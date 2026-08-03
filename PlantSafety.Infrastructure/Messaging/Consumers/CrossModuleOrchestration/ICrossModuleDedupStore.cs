// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration
//  Layer:          Infrastructure
//  File:           ICrossModuleDedupStore.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Business-key dedup ledger abstraction for the 8 Phase 5 §3 cross-module
//                  orchestration consumers. Backed by [System].[CrossModuleDedupKeys] (see
//                  Persistence\Scripts\Tables\CrossModuleDedupKeys.sql for the full rationale).
//                  Deliberately infrastructure-only (mirrors [System].[ProcessedEvents], which
//                  IdempotentConsumerBase also accesses directly without an Application-layer
//                  repository interface) since this is a messaging-hardening concern, not a
//                  domain/application aggregate concern. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;

/// <summary>
/// Business-key dedup ledger used by <see cref="CrossModuleIdempotentConsumerBase{TMessage}"/> to
/// detect a redelivered message BEFORE its side effect re-runs, even when the side effect and the
/// outer <c>[System].[ProcessedEvents]</c> marker write are not part of the same SQL transaction
/// (see <see cref="CrossModuleIdempotentConsumerBase{TMessage}.TryExecuteOnceAsync"/>).
/// </summary>
public interface ICrossModuleDedupStore
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="consumerName"/> has already recorded
    /// <paramref name="businessKey"/> as handled.
    /// </summary>
    Task<bool> ExistsAsync(string consumerName, string businessKey, CancellationToken cancellationToken);

    /// <summary>
    /// Records that <paramref name="consumerName"/> has handled <paramref name="businessKey"/>,
    /// originating from message <paramref name="eventId"/>.
    /// </summary>
    Task RecordAsync(string consumerName, string businessKey, Guid eventId, CancellationToken cancellationToken);
}
