// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for ProceduresCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Procedures.Messaging;

/// <summary>
/// Idempotent consumer for <c>ProceduresCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class ProceduresCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<ProceduresCreatedConsumer> logger)
    : IConsumer<ProceduresCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ProceduresCreatedEvent> context)
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
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed.", "Procedures", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "Procedures", evt.ProceduresId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Procedures", eventId);
            throw;
        }
    }
}
