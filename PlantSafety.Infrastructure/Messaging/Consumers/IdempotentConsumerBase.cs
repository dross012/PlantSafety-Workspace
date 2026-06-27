// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           IdempotentConsumerBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for MassTransit consumers enforcing idempotency via ProcessedEvents table.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers;

/// <summary>
/// Abstract base for MassTransit consumers that enforces idempotency by checking
/// the <c>System.ProcessedEvents</c> table before executing handler logic.
/// </summary>
public abstract class IdempotentConsumerBase<TMessage>(
    ISqlConnectionFactory connectionFactory,
    ILogger logger)
    : IConsumer<TMessage>
    where TMessage : class
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var eventId = ResolveEventId(context.Message);

        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            // Idempotency check
            var alreadyProcessed = await conn.ExecuteScalarAsync<int>(
                sql: $"SELECT COUNT(1) FROM {SqlConstants.ProcessedEventsTable} WHERE EventId = @EventId",
                param: new { EventId = eventId },
                transaction: tx);

            if (alreadyProcessed > 0)
            {
                logger.LogInformation("Event {EventId} already processed — skipping.", eventId);
                return;
            }

            // Execute domain handler
            await HandleAsync(context, conn, tx, context.CancellationToken);

            // Record as processed in same transaction
            await conn.ExecuteAsync(
                sql: $"INSERT INTO {SqlConstants.ProcessedEventsTable} (EventId, ProcessedAt) VALUES (@EventId, @NowUtc)",
                param: new { EventId = eventId, NowUtc = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.LogError(ex, "Consumer failed for event {EventId} — rolled back.", eventId);
            throw;
        }
    }

    /// <summary>Extracts the <c>EventId</c> from the message for idempotency checking.</summary>
    protected abstract Guid ResolveEventId(TMessage message);

    /// <summary>Executes the domain-specific handler within the open transaction.</summary>
    protected abstract Task HandleAsync(
        ConsumeContext<TMessage> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken);
}
