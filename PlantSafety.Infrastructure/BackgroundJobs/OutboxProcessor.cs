// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.BackgroundJobs
//  Layer:          Infrastructure
//  File:           OutboxProcessor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Hosted service that polls OutboxMessages and publishes integration events to Azure Service Bus.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Hosting;

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
        logger.LogInformation("OutboxProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "OutboxProcessor encountered an error during batch processing.");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        logger.LogInformation("OutboxProcessor stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var batch = await conn.QueryAsync<OutboxEnvelope>(
                sql: $"SELECT TOP {InfrastructureConstants.OutboxBatchSize} * FROM {SqlConstants.OutboxTable} WHERE Processed = 0 ORDER BY OccurredAt",
                transaction: tx,
                commandType: CommandType.Text);

            foreach (var envelope in batch)
            {
                await PublishAsync(envelope, cancellationToken);

                await conn.ExecuteAsync(
                    sql: $"UPDATE {SqlConstants.OutboxTable} SET Processed = 1, ProcessedAt = @NowUtc WHERE MessageId = @MessageId",
                    param: new { NowUtc = DateTime.UtcNow, envelope.MessageId },
                    transaction: tx);

                logger.LogInformation("OutboxProcessor published {EventType} MessageId={MessageId}", envelope.EventType, envelope.MessageId);
            }

            tx.Commit();
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
        return bus.Publish(envelope, cancellationToken);
    }
}
