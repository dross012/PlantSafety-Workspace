// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration
//  Layer:          Infrastructure
//  File:           CrossModuleIdempotentConsumerBase.cs
//  Created:        2026-08-02
//  Updated:        2026-08-02 (Phase 6 blocker fix -- true replay atomicity for the 8 cross-module
//                  orchestration consumers; Eventing & Messaging Agent)
//  Author:         PlantSafety Platform Team (Phase 5 §3 Cross-Module Orchestration)
//  SchemaVersion:  2026.2
//  Description:    Shared base for the Phase 5 §3 cross-module orchestration consumers. Extends
//                  the platform's canonical IdempotentConsumerBase (System.ProcessedEvents keyed
//                  by EventId -- see PlantSafety.Infrastructure.Messaging.Consumers) and adds a
//                  MediatR ISender dispatch helper plus canonical outbox-header recovery
//                  (TenantId/ActorId/CorrelationId), since most pre-existing domain events do not
//                  themselves carry TenantId (see OutboxHeaderNames).
//
//                  2026-08-02 UPDATE: each concrete consumer's real side-effect write happens
//                  through MediatR -> an Application command handler -> its own repository -> its
//                  own SQL connection/transaction, which is NOT part of the same transaction as the
//                  outer [System].[ProcessedEvents] marker write performed by
//                  IdempotentConsumerBase.Consume. A crash/redelivery between the two could
//                  otherwise create a genuine duplicate side effect on replay. TryExecuteOnceAsync
//                  closes that gap: it scopes a business-key "check dedup ledger -> perform side
//                  effect -> record dedup ledger" sequence inside an IDistributedLockProvider lock
//                  keyed by the caller-supplied resource key, so (a) a redelivery is detected BEFORE
//                  the side effect re-runs, not merely after, and (b) two concurrent redeliveries of
//                  the same logical event serialize instead of both passing the "not yet handled"
//                  check before either has committed. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Results;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;

/// <summary>
/// Shared base for cross-module orchestration consumers: resolves the propagated tenant/actor
/// context from canonical outbox headers, provides a logging-wrapped MediatR dispatch helper, and
/// provides <see cref="TryExecuteOnceAsync"/> for lock-scoped, business-key-deduplicated side
/// effects (replay-safe even when the side effect is not part of the outer ProcessedEvents
/// transaction).
/// </summary>
public abstract class CrossModuleIdempotentConsumerBase<TMessage>(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger logger)
    : IdempotentConsumerBase<TMessage>(connectionFactory, logger)
    where TMessage : class
{
    /// <summary>Maximum time a consumer will wait to acquire its business-key lock before giving up and letting MassTransit retry/dead-letter the message.</summary>
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    /// <summary>The MediatR sender used to dispatch Application-layer commands into other modules.</summary>
    protected ISender Sender { get; } = sender;

    /// <summary>Structured logger for this consumer.</summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>Dispatches <paramref name="command"/> and logs the outcome; never throws on a domain <c>Result</c> failure.</summary>
    protected async Task<Result<TResponse>> DispatchAsync<TResponse>(
        IRequest<Result<TResponse>> command, string description, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            Logger.LogInformation("CrossModuleOrchestration: {Description} succeeded.", description);
        else
            Logger.LogWarning(
                "CrossModuleOrchestration: {Description} failed: [{ErrorCode}] {Message}",
                description, result.Error.ErrorCode, result.Error.Message);

        return result;
    }

    /// <summary>
    /// Executes <paramref name="sideEffect"/> at most once for a given (consumer, <paramref
    /// name="businessKey"/>) pair, even across message redelivery, by acquiring an
    /// <see cref="IDistributedLockProvider"/> lock on <paramref name="lockResourceKey"/>, checking
    /// the <c>[System].[CrossModuleDedupKeys]</c> ledger for a prior recording, running
    /// <paramref name="sideEffect"/> only if none exists, and recording the ledger entry
    /// immediately afterwards -- all while still holding the lock, so a concurrent redelivery
    /// cannot observe "not yet handled" until this attempt has either recorded success or thrown.
    /// </summary>
    /// <param name="lockResourceKey">
    /// Distributed-lock resource key, namespaced per consumer/business-entity (e.g.
    /// <c>"CrossModule:RiskCapa:{RiskId}"</c>).
    /// </param>
    /// <param name="businessKey">
    /// The business identity deduplicated against (e.g. the triggering alert id, completed task
    /// id, or source event id) -- distinct from the message's own <c>EventId</c>, which
    /// <see cref="IdempotentConsumerBase{TMessage}"/> already covers at the envelope level.
    /// </param>
    /// <param name="eventId">The inbound message's own EventId, recorded alongside the ledger entry for diagnostics.</param>
    /// <param name="sideEffect">The side-effecting delegate to run at most once.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns><see langword="true"/> if <paramref name="sideEffect"/> ran; <see langword="false"/> if it was skipped as an already-handled duplicate.</returns>
    protected async Task<bool> TryExecuteOnceAsync(
        string lockResourceKey,
        string businessKey,
        Guid eventId,
        Func<Task> sideEffect,
        CancellationToken cancellationToken)
    {
        var consumerName = GetType().Name;

        await using var lockHandle = await lockProvider.AcquireAsync(lockResourceKey, LockTimeout, cancellationToken);

        if (await dedupStore.ExistsAsync(consumerName, businessKey, cancellationToken))
        {
            Logger.LogInformation(
                "CrossModuleOrchestration: {Consumer} business key {BusinessKey} already handled -- skipping duplicate side effect on replay.",
                consumerName, businessKey);
            return false;
        }

        await sideEffect();

        await dedupStore.RecordAsync(consumerName, businessKey, eventId, cancellationToken);
        return true;
    }
}
