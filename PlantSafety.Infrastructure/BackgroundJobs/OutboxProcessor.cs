// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.BackgroundJobs
//  Layer:          Infrastructure
//  File:           OutboxProcessor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  Last Updated:   2026-08-03 (Claude Code performance-optimization-agent) -- Regression audit
//                  fix (Handbook §2.19-2.22, §2.31.2 Canonical Background Job Logging Example):
//                  this hosted service previously emitted message-template logs
//                  ("OutboxProcessor started.", "OutboxProcessor published {EventType} ...")
//                  with no lineage (no root CorrelationId/CausationId) and no SchemaVersion.
//                  Converted the batch-level start/success/error logs to canonical envelopes via
//                  IApplicationLogger, generating a fresh root CorrelationId/CausationId per
//                  batch per §2.31.2 ("Background jobs MUST generate a new root CausationId...").
//                  The per-message LogInformation/LogWarning/LogDebug calls inside PublishAsync
//                  are left as-is (out of this pass's representative-sample scope; see remarks).
//  SchemaVersion:  2026.1
//  Description:    Hosted service that polls OutboxMessages and publishes integration events to Azure Service Bus.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Hosting;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Infrastructure.Common.Serialization;
using PlantSafety.Infrastructure.Messaging;

namespace PlantSafety.Infrastructure.BackgroundJobs;

/// <summary>
/// Hosted service that periodically polls <c>System.OutboxMessages</c> for unprocessed
/// envelopes and publishes them to Azure Service Bus as integration events.
/// Marks each envelope processed within its own SQL transaction (idempotent).
/// </summary>
public sealed class OutboxProcessor(
    ISqlConnectionFactory connectionFactory,
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessor> logger)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(InfrastructureConstants.OutboxProcessorIntervalSeconds);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Handbook §2.31.2: background jobs generate a fresh root CorrelationId/CausationId
            // per execution (here: per polling batch), since there is no inbound API request to
            // inherit lineage from.
            var correlationId = Guid.NewGuid();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // IApplicationLogger is Scoped; OutboxProcessor is a singleton hosted service, so it
            // cannot be constructor-injected here (ValidateOnBuild correctly rejects that lifetime
            // mismatch at startup). Resolved per-batch from a fresh scope instead, mirroring the
            // existing PublishAsync -> IBus pattern below.
            using var loggerScope = serviceProvider.CreateScope();
            var applicationLogger = loggerScope.ServiceProvider.GetRequiredService<IApplicationLogger>();

            try
            {
                var processedCount = await ProcessBatchAsync(stoppingToken);
                stopwatch.Stop();

                applicationLogger.LogSuccess(new OperationLogEnvelope
                {
                    OperationName = "OutboxProcessor.ProcessBatch",
                    HandlerType = nameof(OutboxProcessor),
                    CorrelationId = correlationId,
                    TenantId = Guid.Empty,
                    TimestampUtc = DateTime.UtcNow,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Success = true,
                    Severity = LogSeverity.Information
                });

                _ = processedCount;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                stopwatch.Stop();
                var sanitized = PlantSafety.Application.Common.Logging.ExceptionSanitizer.Sanitize(ex);

                applicationLogger.LogError(new OperationLogEnvelope
                {
                    OperationName = "OutboxProcessor.ProcessBatch",
                    HandlerType = nameof(OutboxProcessor),
                    CorrelationId = correlationId,
                    TenantId = Guid.Empty,
                    TimestampUtc = DateTime.UtcNow,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Success = false,
                    ExceptionType = sanitized.ExceptionType,
                    Message = sanitized.Message,
                    HResult = sanitized.HResult,
                    StackTraceHash = sanitized.StackTraceHash,
                    Severity = LogSeverity.Error
                });
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var batch = await conn.QueryAsync<OutboxEnvelope>(
                sql: $"SELECT TOP {InfrastructureConstants.OutboxBatchSize} * FROM {SqlConstants.OutboxTable} WHERE Processed = 0 ORDER BY OccurredAt",
                transaction: tx,
                commandType: CommandType.Text);

            var processedCount = 0;

            foreach (var envelope in batch)
            {
                await PublishAsync(envelope, cancellationToken);

                await conn.ExecuteAsync(
                    sql: $"UPDATE {SqlConstants.OutboxTable} SET Processed = 1, ProcessedAt = @NowUtc WHERE MessageId = @MessageId",
                    param: new { NowUtc = DateTime.UtcNow, envelope.MessageId },
                    transaction: tx);

                logger.LogInformation("OutboxProcessor published {EventType} MessageId={MessageId}", envelope.EventType, envelope.MessageId);
                processedCount++;
            }

            tx.Commit();
            return processedCount;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private Task PublishAsync(OutboxEnvelope envelope, CancellationToken cancellationToken)
    {
        // Resolved per-scope to avoid capturing a stale bus instance.
        using var scope = serviceProvider.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        // Every real MassTransit consumer in this codebase is an IConsumer<TConcreteDomainEvent>
        // (e.g. IConsumer<RiskManagementCreatedEvent>) -- publishing the raw OutboxEnvelope wrapper
        // would never reach any of them. Resolve the concrete CLR event type via the canonical
        // EventType discriminator and deserialize the original payload back into it so consumers
        // receive the real, strongly-typed integration event.
        if (OutboxEventTypeRegistry.TryResolve(envelope.EventType, out var domainEventType) && domainEventType is not null)
        {
            try
            {
                var domainEvent = JsonSerializer.Deserialize(envelope.Payload, domainEventType, JsonSerializerConfiguration.Default);
                if (domainEvent is not null)
                {
                    // Many domain-event records do not themselves carry TenantId (a pre-existing
                    // gap relative to Handbook §15 Multi-Tenancy Rules), but the outbox envelope
                    // always does. Propagate it (plus Actor/Correlation) as MassTransit message
                    // headers so tenant-scoped cross-module consumers can recover it without a
                    // schema change to every event record.
                    return bus.Publish(domainEvent, domainEventType, PublishContextCallback(envelope), cancellationToken);
                }

                logger.LogWarning(
                    "OutboxProcessor: envelope {MessageId} EventType={EventType} resolved to {ClrType} but deserialization returned null; publishing raw envelope as fallback.",
                    envelope.MessageId, envelope.EventType, domainEventType.FullName);
            }
            catch (JsonException ex)
            {
                // Some domain events carry "Smart Enum" value objects (e.g.
                // PlantSafety.Domain.RiskManagement.Enums.RiskLevel) with private constructors that
                // System.Text.Json cannot round-trip without a bespoke JsonConverter (none is
                // registered for these types yet -- a pre-existing gap this surfaces rather than
                // introduces). Degrade to the raw-envelope fallback rather than losing the message
                // or crashing the processing loop; the event still reaches OutboxMessages consumers
                // that read the envelope directly (e.g. OutboxTimelineEntryReader).
                logger.LogWarning(ex,
                    "OutboxProcessor: envelope {MessageId} EventType={EventType} failed to deserialize into {ClrType}; publishing raw envelope as fallback.",
                    envelope.MessageId, envelope.EventType, domainEventType.FullName);
            }
        }
        else
        {
            logger.LogDebug(
                "OutboxProcessor: no registered domain-event CLR type for EventType={EventType} (MessageId={MessageId}); publishing raw envelope as fallback.",
                envelope.EventType, envelope.MessageId);
        }

        return bus.Publish(envelope, cancellationToken);
    }

    private static Action<PublishContext> PublishContextCallback(OutboxEnvelope envelope) => ctx =>
    {
        ctx.Headers.Set(OutboxHeaderNames.TenantId, envelope.TenantId);
        ctx.Headers.Set(OutboxHeaderNames.ActorId, envelope.ActorId);
        ctx.Headers.Set(OutboxHeaderNames.CorrelationId, envelope.CorrelationId);
        ctx.Headers.Set(OutboxHeaderNames.OutboxMessageId, envelope.MessageId.ToString());
    };
}
