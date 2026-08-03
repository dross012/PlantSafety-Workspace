// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration
//  Layer:          Infrastructure
//  File:           SqlCrossModuleDedupStore.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    SQL Server-backed <see cref="ICrossModuleDedupStore"/> against
//                  [System].[CrossModuleDedupKeys]. Uses raw parameterized SQL (no stored
//                  procedure), mirroring IdempotentConsumerBase's own access pattern against the
//                  sibling [System].[ProcessedEvents] table -- both are messaging-infrastructure
//                  idempotency ledgers, not domain/application aggregates, so the platform's
//                  canonical stored-procedure/TRY-CATCH convention (reserved for domain
//                  Repositories, see Handbook §2.10) does not apply here. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;

/// <summary>
/// <see cref="ICrossModuleDedupStore"/> implementation backed by
/// <c>[System].[CrossModuleDedupKeys]</c>.
/// </summary>
public sealed class SqlCrossModuleDedupStore(
    ISqlConnectionFactory connectionFactory,
    ILogger<SqlCrossModuleDedupStore> logger) : ICrossModuleDedupStore
{
    private const string TableName = "[System].[CrossModuleDedupKeys]";

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string consumerName, string businessKey, CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();

        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                $"SELECT COUNT(1) FROM {TableName} WHERE ConsumerName = @ConsumerName AND BusinessKey = @BusinessKey",
                new { ConsumerName = consumerName, BusinessKey = businessKey },
                cancellationToken: cancellationToken));

        return count > 0;
    }

    /// <inheritdoc/>
    public async Task RecordAsync(string consumerName, string businessKey, Guid eventId, CancellationToken cancellationToken)
    {
        using var conn = connectionFactory.CreateConnection();

        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                $"INSERT INTO {TableName} (ConsumerName, BusinessKey, EventId, ProcessedAtUtc) VALUES (@ConsumerName, @BusinessKey, @EventId, @NowUtc)",
                new { ConsumerName = consumerName, BusinessKey = businessKey, EventId = eventId, NowUtc = DateTime.UtcNow },
                cancellationToken: cancellationToken));
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // Unique-constraint violation on (ConsumerName, BusinessKey): a concurrent redelivery
            // already recorded this business key between our ExistsAsync check and this insert.
            // Under normal operation this path is unreachable because callers serialize the full
            // check-then-act sequence behind an IDistributedLockProvider lock (see
            // CrossModuleIdempotentConsumerBase.TryExecuteOnceAsync); treat it as a benign race
            // rather than a failure if it ever occurs (e.g. the lock provider degraded).
            logger.LogWarning(
                "CrossModuleDedupKeys: concurrent duplicate insert for {ConsumerName}/{BusinessKey} -- already recorded.",
                consumerName, businessKey);
        }
    }
}
