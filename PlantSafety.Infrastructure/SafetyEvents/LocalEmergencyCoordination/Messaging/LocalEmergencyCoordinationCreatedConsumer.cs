// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Infrastructure
//  File:           LocalEmergencyCoordinationCreatedConsumer.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for LocalEmergencyCoordinationCreatedEvent. Also the
//                  landing point that a future PublicSafetyCommunication module can model its own
//                  consumer after, since ILocalEmergencyCoordinationDomainEvent (and every event
//                  implementing it) is published to the outbox regardless of whether such a
//                  downstream consumer exists yet in this repository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;

namespace PlantSafety.Infrastructure.SafetyEvents.LocalEmergencyCoordination.Messaging;

/// <summary>
/// Idempotent consumer for <c>LocalEmergencyCoordinationCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class LocalEmergencyCoordinationCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<LocalEmergencyCoordinationCreatedConsumer> logger)
    : IConsumer<LocalEmergencyCoordinationCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<LocalEmergencyCoordinationCreatedEvent> context)
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
                logger.LogInformation("{Mod} event {EventId} already processed.", "LocalEmergencyCoordination", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created — event consumed.", "LocalEmergencyCoordination", evt.LocalEmergencyCoordinationId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "LocalEmergencyCoordination", eventId);
            throw;
        }
    }
}
