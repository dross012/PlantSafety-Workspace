// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for ComplianceCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Compliance.Messaging;

/// <summary>
/// Idempotent consumer for <c>ComplianceCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class ComplianceCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<ComplianceCreatedConsumer> logger)
    : IConsumer<ComplianceCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ComplianceCreatedEvent> context)
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
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed.", "Compliance", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "Compliance", evt.ComplianceId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Compliance", eventId);
            throw;
        }
    }
}
