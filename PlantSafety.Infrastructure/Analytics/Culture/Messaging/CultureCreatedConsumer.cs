// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for CultureCreatedEvent. Idempotency enforced via ProcessedEvents table.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Culture.Messaging;

/// <summary>
/// Idempotent consumer for <c>CultureCreatedEvent</c> integration events.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class CultureCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<CultureCreatedConsumer> logger)
    : IConsumer<CultureCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<CultureCreatedEvent> context)
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
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed — skipping.", "Culture", eventId);
                return;
            }

            // TODO: execute Culture-specific side-effects here (projections, downstream triggers, etc.)
            logger.LogInformation("{Mod} {Id} created event consumed.", "Culture", evt.CultureId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Culture", eventId);
            throw;
        }
    }
}
