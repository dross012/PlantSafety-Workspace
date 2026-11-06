// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           SqlEventSubscriber.cs
//  Created:        2026-07-27
//  Description:    IEventSubscriber backed by dbo.EventQueue / ProcessedEvents /
//                  EventRetryState / EventDeadLetterQueue. Implements the consumer-hardening
//                  rules from project_plantsafety_patterns.md §12:
//                    - Idempotency:        ProcessedEvents keyed by (ConsumerGroup, EventId)
//                    - Retry strategy:     bounded retries with exponential backoff
//                    - Dead-letter queue:  unresolvable messages moved with full context
//                    - Structured logging: EventId/CorrelationId/ConsumerGroup/Outcome
//                  No EF -- raw parameterised SQL via Dapper against a single small shared
//                  schema, consistent with Shared.AuditLogging.Infrastructure's precedent
//                  for cross-module shared tables.
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Abstractions.Handlers;
using Shared.EventBus.Infrastructure.Options;

namespace Shared.EventBus.Infrastructure.Dispatch;

/// <summary>SQL-queue-backed <see cref="IEventSubscriber"/> for one consumer group.</summary>
public sealed class SqlEventSubscriber : IEventSubscriber
{
    private readonly EventHandlerRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EventBusOptions _options;
    private readonly ILogger<SqlEventSubscriber> _logger;

    /// <inheritdoc/>
    public string ConsumerGroup { get; }

    public SqlEventSubscriber(
        string consumerGroup,
        EventHandlerRegistry registry,
        IServiceScopeFactory scopeFactory,
        IOptions<EventBusOptions> options,
        ILogger<SqlEventSubscriber> logger)
    {
        ConsumerGroup = consumerGroup;
        _registry = registry;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<int> ProcessDueEventsAsync(int maxBatchSize = 50, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var dueRows = (await connection.QueryAsync<EventQueueRow>(new CommandDefinition(
            DueEventsSql,
            new { ConsumerGroup, NowUtc = DateTime.UtcNow, MaxBatchSize = maxBatchSize },
            commandTimeout: _options.CommandTimeoutSeconds,
            cancellationToken: cancellationToken))).ToList();

        var processedCount = 0;

        foreach (var row in dueRows)
        {
            await ProcessOneAsync(connection, row, cancellationToken);
            processedCount++;
        }

        return processedCount;
    }

    private async Task ProcessOneAsync(SqlConnection connection, EventQueueRow row, CancellationToken cancellationToken)
    {
        var envelope = new EventEnvelope
        {
            EventId = row.EventId,
            EventType = row.EventType,
            SchemaVersion = row.SchemaVersion,
            PayloadType = row.PayloadType,
            TenantId = row.TenantId,
            CorrelationId = row.CorrelationId,
            CausationId = row.CausationId,
            OccurredAtUtc = row.OccurredAtUtc,
            SourceModule = row.SourceModule,
            Payload = row.Payload,
            EnqueuedAtUtc = row.EnqueuedAtUtc,
        };

        var descriptor = _registry.Find(ConsumerGroup, row.PayloadType);
        if (descriptor is null)
        {
            // Not relevant to this consumer group -- mark processed so it is never re-scanned.
            await MarkProcessedAsync(connection, row.EventId, cancellationToken);
            _logger.LogDebug(
                "EventBus[{ConsumerGroup}]: no handler for {PayloadType} (EventId={EventId}); marked processed as not-applicable.",
                ConsumerGroup, row.PayloadType, row.EventId);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            await descriptor.Invoke(scope.ServiceProvider, envelope, cancellationToken);

            await MarkProcessedAsync(connection, row.EventId, cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(
                DeleteRetryStateSql, new { ConsumerGroup, row.EventId },
                commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));

            _logger.LogInformation(
                "EventBus[{ConsumerGroup}]: handled {EventType} (EventId={EventId}, CorrelationId={CorrelationId}). Outcome=Success.",
                ConsumerGroup, row.EventType, row.EventId, row.CorrelationId);
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(connection, row, ex, cancellationToken);
        }
    }

    private async Task HandleFailureAsync(SqlConnection connection, EventQueueRow row, Exception ex, CancellationToken cancellationToken)
    {
        var existing = await connection.QuerySingleOrDefaultAsync<RetryStateRow>(new CommandDefinition(
            SelectRetryStateSql, new { ConsumerGroup, row.EventId },
            commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));

        var attemptCount = (existing?.AttemptCount ?? 0) + 1;
        var errorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
        var isRepeatedFailure = existing is not null &&
                                 string.Equals(existing.LastError, errorMessage, StringComparison.Ordinal);

        _logger.LogWarning(ex,
            "EventBus[{ConsumerGroup}]: failed to handle {EventType} (EventId={EventId}, CorrelationId={CorrelationId}), attempt {AttemptCount}/{MaxAttempts}. Outcome=Failed.",
            ConsumerGroup, row.EventType, row.EventId, row.CorrelationId, attemptCount, _options.MaxDeliveryAttempts);

        if (attemptCount >= _options.MaxDeliveryAttempts)
        {
            var isPoison = isRepeatedFailure && attemptCount >= _options.PoisonMessageRepeatThreshold;

            await connection.ExecuteAsync(new CommandDefinition(
                InsertDeadLetterSql,
                new
                {
                    ConsumerGroup,
                    row.EventId,
                    row.EventType,
                    row.PayloadType,
                    row.TenantId,
                    row.CorrelationId,
                    row.Payload,
                    AttemptCount = attemptCount,
                    FailureReason = errorMessage,
                    IsPoisonMessage = isPoison,
                },
                commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                DeleteRetryStateSql, new { ConsumerGroup, row.EventId },
                commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));

            // Dead-lettered messages still count as "processed" for the queue scan so they
            // are not picked up again -- diagnostics live in EventDeadLetterQueue instead.
            await MarkProcessedAsync(connection, row.EventId, cancellationToken);

            _logger.LogError(
                "EventBus[{ConsumerGroup}]: {EventType} (EventId={EventId}) exceeded {MaxAttempts} attempts. Moved to dead-letter queue. IsPoisonMessage={IsPoisonMessage}. Outcome=DeadLettered.",
                ConsumerGroup, row.EventType, row.EventId, _options.MaxDeliveryAttempts, isPoison);
            return;
        }

        var nextAttemptAtUtc = DateTime.UtcNow.Add(_options.ComputeBackoff(attemptCount));

        await connection.ExecuteAsync(new CommandDefinition(
            UpsertRetryStateSql,
            new { ConsumerGroup, row.EventId, AttemptCount = attemptCount, LastAttemptAtUtc = DateTime.UtcNow, NextAttemptAtUtc = nextAttemptAtUtc, LastError = errorMessage },
            commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));
    }

    private Task MarkProcessedAsync(SqlConnection connection, Guid eventId, CancellationToken cancellationToken)
        => connection.ExecuteAsync(new CommandDefinition(
            MarkProcessedSql, new { ConsumerGroup, EventId = eventId },
            commandTimeout: _options.CommandTimeoutSeconds, cancellationToken: cancellationToken));

    // Due = not yet in ProcessedEvents for this consumer group, and either never attempted
    // or its NextAttemptAtUtc backoff window has elapsed.
    private const string DueEventsSql = """
        SELECT TOP (@MaxBatchSize) q.EventId, q.EventType, q.SchemaVersion, q.PayloadType, q.TenantId,
               q.CorrelationId, q.CausationId, q.OccurredAtUtc, q.SourceModule, q.Payload, q.EnqueuedAtUtc
        FROM dbo.EventQueue q
        LEFT JOIN dbo.ProcessedEvents p ON p.ConsumerGroup = @ConsumerGroup AND p.EventId = q.EventId
        LEFT JOIN dbo.EventRetryState r ON r.ConsumerGroup = @ConsumerGroup AND r.EventId = q.EventId
        WHERE p.EventId IS NULL
          AND (r.EventId IS NULL OR r.NextAttemptAtUtc <= @NowUtc)
        ORDER BY q.EnqueuedAtUtc ASC;
        """;

    private const string MarkProcessedSql = """
        IF NOT EXISTS (SELECT 1 FROM dbo.ProcessedEvents WHERE ConsumerGroup = @ConsumerGroup AND EventId = @EventId)
            INSERT INTO dbo.ProcessedEvents (ConsumerGroup, EventId, ProcessedAtUtc) VALUES (@ConsumerGroup, @EventId, SYSUTCDATETIME());
        """;

    private const string SelectRetryStateSql = """
        SELECT AttemptCount, LastAttemptAtUtc, NextAttemptAtUtc, LastError
        FROM dbo.EventRetryState WHERE ConsumerGroup = @ConsumerGroup AND EventId = @EventId;
        """;

    private const string UpsertRetryStateSql = """
        IF EXISTS (SELECT 1 FROM dbo.EventRetryState WHERE ConsumerGroup = @ConsumerGroup AND EventId = @EventId)
            UPDATE dbo.EventRetryState
               SET AttemptCount = @AttemptCount, LastAttemptAtUtc = @LastAttemptAtUtc,
                   NextAttemptAtUtc = @NextAttemptAtUtc, LastError = @LastError
             WHERE ConsumerGroup = @ConsumerGroup AND EventId = @EventId;
        ELSE
            INSERT INTO dbo.EventRetryState (ConsumerGroup, EventId, AttemptCount, LastAttemptAtUtc, NextAttemptAtUtc, LastError)
            VALUES (@ConsumerGroup, @EventId, @AttemptCount, @LastAttemptAtUtc, @NextAttemptAtUtc, @LastError);
        """;

    private const string DeleteRetryStateSql = """
        DELETE FROM dbo.EventRetryState WHERE ConsumerGroup = @ConsumerGroup AND EventId = @EventId;
        """;

    private const string InsertDeadLetterSql = """
        INSERT INTO dbo.EventDeadLetterQueue
            (DeadLetterId, ConsumerGroup, EventId, EventType, PayloadType, TenantId, CorrelationId,
             Payload, AttemptCount, FailureReason, IsPoisonMessage, DeadLetteredAtUtc)
        VALUES
            (NEWID(), @ConsumerGroup, @EventId, @EventType, @PayloadType, @TenantId, @CorrelationId,
             @Payload, @AttemptCount, @FailureReason, @IsPoisonMessage, SYSUTCDATETIME());
        """;

    private sealed class EventQueueRow
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public int SchemaVersion { get; init; }
        public string PayloadType { get; init; } = string.Empty;
        public string TenantId { get; init; } = string.Empty;
        public Guid CorrelationId { get; init; }
        public Guid CausationId { get; init; }
        public DateTime OccurredAtUtc { get; init; }
        public string SourceModule { get; init; } = string.Empty;
        public string Payload { get; init; } = string.Empty;
        public DateTime EnqueuedAtUtc { get; init; }
    }

    private sealed class RetryStateRow
    {
        public int AttemptCount { get; init; }
        public DateTime? LastAttemptAtUtc { get; init; }
        public DateTime NextAttemptAtUtc { get; init; }
        public string? LastError { get; init; }
    }
}
