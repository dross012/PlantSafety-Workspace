// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Infrastructure
//  File:           RegulatoryCalendarCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for RegulatoryCalendarCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Messaging;

/// <summary>
/// Idempotent consumer for <c>RegulatoryCalendarCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class RegulatoryCalendarCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<RegulatoryCalendarCreatedConsumer> logger)
    : IConsumer<RegulatoryCalendarCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<RegulatoryCalendarCreatedEvent> context)
    {
        var evt     = context.Message;
        var eventId = evt.EventId.Value;

        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var alreadyProcessed = await conn.ExecuteScalarAsync<int>(
                sql: \$"SELECT COUNT(1) FROM {SqlConstants.ProcessedEventsTable} WHERE EventId = @EventId",
                param: new { EventId = eventId },
                transaction: tx);

            if (alreadyProcessed > 0)
            {
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed.", "RegulatoryCalendar", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "RegulatoryCalendar", evt.RegulatoryCalendarId);

            await conn.ExecuteAsync(
                sql: \$"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "RegulatoryCalendar", eventId);
            throw;
        }
    }
}
