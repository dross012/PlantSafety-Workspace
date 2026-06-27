// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training.CompetencyManagement
//  Layer:          Infrastructure
//  File:           CompetencyManagementCreatedConsumer.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer for CompetencyManagementCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Training.CompetencyManagement.Messaging;

/// <summary>Idempotent consumer for <c>CompetencyManagementCreatedEvent</c>.</summary>
public sealed class CompetencyManagementCreatedConsumer(
    ISqlConnectionFactory connectionFactory,
    ILogger<CompetencyManagementCreatedConsumer> logger)
    : IConsumer<CompetencyManagementCreatedEvent>
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<CompetencyManagementCreatedEvent> context)
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
                logger.LogInformation("CompetencyManagement event {EventId} already processed.", eventId);
                return;
            }

            logger.LogInformation("CompetencyManagement {Id} created — event consumed.", evt.CompetencyManagementId);

            await conn.ExecuteAsync(
                sql: \$"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "CompetencyManagementCreatedConsumer failed for event {EventId}.", eventId);
            throw;
        }
    }
}
