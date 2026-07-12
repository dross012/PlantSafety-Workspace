// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentManagementCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for IncidentManagementCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Messaging;

/// <summary>
/// Idempotent consumer for <c>IncidentManagementCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class IncidentManagementCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<IncidentManagementCreatedConsumer> logger)
    : IConsumer<IncidentManagementCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<IncidentManagementCreatedEvent> context)
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
                logger.LogInformation("{Mod} event {EventId} already processed.", "IncidentManagement", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created — event consumed.", "IncidentManagement", evt.IncidentManagementId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "IncidentManagement", eventId);
            throw;
        }
    }
}
