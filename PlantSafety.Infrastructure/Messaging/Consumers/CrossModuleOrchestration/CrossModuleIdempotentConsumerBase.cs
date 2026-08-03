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
//
//                  2026-08-03 UPDATE (Phase 7 §1.3 Tenant-Aware Eventing): added
//                  EnsureExpectedTenant, a consumer-side tenant-boundary check mirroring the
//                  Application-layer TenantIsolationBehavior's defense-in-depth pattern. Every
//                  cross-module orchestration consumer already recovers the propagated TenantId
//                  from the canonical outbox headers (ResolveEnvelopeTenantId) before dispatching
//                  into another module's command handler; this guard makes verifying that the
//                  recovered TenantId matches the expected/allowed tenant context a single,
//                  reusable call instead of leaving it to each of the 8 concrete consumers to
//                  remember independently. Additive-only evolution.
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

    /// <summary>
    /// Defense-in-depth tenant-boundary check (Phase 7 §1.3 Tenant-Aware Eventing), mirroring
    /// <c>PlantSafety.Application.Behaviors.Security.TenantIsolationBehavior</c>'s API-layer
    /// guarantee at the consumer boundary: verifies that <paramref name="envelopeTenantId"/> (the
    /// TenantId recovered from the inbound message's propagated outbox headers, see
    /// <see cref="OutboxHeaderExtensions.GetEnvelopeTenantId"/>) matches
    /// <paramref name="expectedTenantId"/> -- the tenant context the concrete consumer is about to
    /// act within (e.g. the TenantId already resolved from the business record the event references)
    /// -- before any side effect is dispatched. Throws rather than silently dropping the message: a
    /// mismatch here indicates either a message-bus misconfiguration or a genuine cross-tenant data
    /// leak upstream, either of which must surface loudly (dead-lettered/retried), never be
    /// swallowed. A <see cref="Guid.Empty"/> <paramref name="expectedTenantId"/> means the concrete
    /// consumer has not yet resolved an expected tenant to compare against (e.g. before the
    /// triggering record has been loaded) and the check is skipped for that call -- callers should
    /// invoke this again once the expected tenant is known, if the record load itself does not
    /// already enforce tenant scoping (it does, via the existing @TenantId stored-procedure
    /// parameter convention, so this is additional defense-in-depth, not the sole guard).
    /// </summary>
    /// <param name="envelopeTenantId">The TenantId recovered from the inbound message's propagated outbox headers.</param>
    /// <param name="expectedTenantId">The tenant context the consumer is about to act within, or <see cref="Guid.Empty"/> if not yet known.</param>
    /// <param name="eventId">The inbound message's own EventId, recorded in the exception for diagnostics.</param>
    /// <exception cref="CrossModuleTenantBoundaryViolationException">
    /// Thrown when both ids are non-empty and do not match.
    /// </exception>
    protected void EnsureExpectedTenant(Guid envelopeTenantId, Guid expectedTenantId, Guid eventId)
    {
        if (envelopeTenantId == Guid.Empty || expectedTenantId == Guid.Empty)
        {
            return;
        }

        if (envelopeTenantId != expectedTenantId)
        {
            var consumerName = GetType().Name;

            Logger.LogWarning(
                "CrossModuleOrchestration: {Consumer} rejected event {EventId} -- envelope TenantId " +
                "{EnvelopeTenantId} does not match expected TenantId {ExpectedTenantId}.",
                consumerName, eventId, envelopeTenantId, expectedTenantId);

            throw new CrossModuleTenantBoundaryViolationException(consumerName, eventId, envelopeTenantId, expectedTenantId);
        }
    }
}

/// <summary>
/// Thrown by <see cref="CrossModuleIdempotentConsumerBase{TMessage}.EnsureExpectedTenant"/> when an
/// inbound cross-module event's propagated TenantId does not match the tenant context the consumer
/// resolved from the business record the event references -- a cross-tenant boundary violation
/// caught at the consumer, in addition to the API-layer <c>TenantIsolationBehavior</c> guard.
/// </summary>
public sealed class CrossModuleTenantBoundaryViolationException : Exception
{
    /// <summary>Name of the consumer type that raised the violation.</summary>
    public string ConsumerName { get; }

    /// <summary>The inbound message's own EventId.</summary>
    public Guid EventId { get; }

    /// <summary>TenantId recovered from the inbound message's propagated outbox headers.</summary>
    public Guid EnvelopeTenantId { get; }

    /// <summary>The tenant context the consumer expected, resolved independently from the referenced business record.</summary>
    public Guid ExpectedTenantId { get; }

    /// <summary>Initializes a new instance of <see cref="CrossModuleTenantBoundaryViolationException"/>.</summary>
    public CrossModuleTenantBoundaryViolationException(string consumerName, Guid eventId, Guid envelopeTenantId, Guid expectedTenantId)
        : base($"Consumer '{consumerName}' rejected event '{eventId}': envelope TenantId '{envelopeTenantId}' " +
               $"does not match expected TenantId '{expectedTenantId}'.")
    {
        ConsumerName = consumerName;
        EventId = eventId;
        EnvelopeTenantId = envelopeTenantId;
        ExpectedTenantId = expectedTenantId;
    }
}
