// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Infrastructure
//  File:           PolicyCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for PolicyCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Policies.Messaging;

/// <summary>
/// Idempotent consumer for <c>PolicyCreatedEvent</c>.
/// Checks <c>System.ProcessedEvents</c> before executing side-effects.
/// </summary>
public sealed class PolicyCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<PolicyCreatedConsumer> logger)
    : IConsumer<PolicyCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<PolicyCreatedEvent> context)
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
                logger.LogInformation("{Mod}CreatedConsumer: event {EventId} already processed.", "Policy", eventId);
                return;
            }

            logger.LogInformation("{Mod} {Id} created event consumed.", "Policy", evt.PolicyId);

            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "{Mod}CreatedConsumer failed for event {EventId}.", "Policy", eventId);
            throw;
        }
    }
}
