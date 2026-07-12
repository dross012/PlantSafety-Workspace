// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Checklists
//  Layer:          Infrastructure
//  File:           ChecklistsCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for ChecklistsCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Checklists.Messaging;

/// <summary>
/// Idempotent MassTransit consumer for <c>ChecklistsCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class ChecklistsCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<ChecklistsCreatedConsumer> logger)
    : IConsumer<ChecklistsCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ChecklistsCreatedEvent> context)
    {
        var evt     = context.Message;
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
                logger.LogInformation("{Mod} event {EventId} already processed — skipping.", "Checklists", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "Checklists", evt.ChecklistsId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Checklists", eventId);
            throw;
        }
    }
}
