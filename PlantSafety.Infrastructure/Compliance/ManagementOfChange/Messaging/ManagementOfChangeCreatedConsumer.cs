// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           ManagementOfChangeCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for ManagementOfChangeCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.Messaging;

/// <summary>
/// Idempotent consumer for <c>ManagementOfChangeCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class ManagementOfChangeCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<ManagementOfChangeCreatedConsumer> logger)
    : IConsumer<ManagementOfChangeCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ManagementOfChangeCreatedEvent> context)
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
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed.", "ManagementOfChange", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "ManagementOfChange", evt.ManagementOfChangeId);

            await conn.ExecuteAsync(
                sql: \$"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "ManagementOfChange", eventId);
            throw;
        }
    }
}
