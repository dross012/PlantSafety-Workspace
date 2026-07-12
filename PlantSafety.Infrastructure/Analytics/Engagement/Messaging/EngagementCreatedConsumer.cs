// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for EngagementCreatedEvent. Idempotency enforced via ProcessedEvents table.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Engagement.Messaging;

/// <summary>
/// Idempotent consumer for <c>EngagementCreatedEvent</c> integration events.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class EngagementCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<EngagementCreatedConsumer> logger)
    : IConsumer<EngagementCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<EngagementCreatedEvent> context)
    {
        var evt    = context.Message;
        var eventId = evt.EventId.Value;

        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var alreadyProcessed = await conn.ExecuteScalarAsync<int>(
                sql: $"SELECT COUNT(1) FROM {SqlConstants.ProcessedEventsTable} WHERE EventId = @EventId",
                param: new { EventId = eventId },
                transaction: tx);

            if (alreadyProcessed > 0)
            {
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed — skipping.", "Engagement", eventId);
                return;
            }

            // TODO: execute Engagement-specific side-effects here (projections, downstream triggers, etc.)
            logger.LogInformation("{Mod} {Id} created event consumed.", "Engagement", evt.EngagementId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Engagement", eventId);
            throw;
        }
    }
}
