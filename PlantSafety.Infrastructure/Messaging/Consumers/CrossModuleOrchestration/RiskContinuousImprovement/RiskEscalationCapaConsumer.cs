// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.RiskContinuousImprovement
//  Layer:          Infrastructure
//  File:           RiskEscalationCapaConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.3 RiskManagement <-> ContinuousImprovement)
//  SchemaVersion:  2026.1
//  Description:    Direction 1 of §3.3: when a hazard is scored High or Critical
//                  (RiskManagement.Events.RiskScoredEvent, Full Risk Management Process Phase 1
//                  Step 2), automatically open a real CAPA CorrectiveAction via the existing
//                  ICorrectiveActionRepository.CreateAsync surface (the same repository
//                  CreateCorrectiveActionCommandHandler uses), tagging
//                  SourceEventType = "RiskManagement.Risk" / SourceEventId = the RiskId so the
//                  feedback direction (CapaCompletionRiskReductionConsumer) can resolve the
//                  originating risk without a schema change.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): the CorrectiveAction-repository
//                  business-key existence check below (by TenantId + SourceEventId + SourceEventType)
//                  already defends against duplicate CAPA creation on replay, but was not previously
//                  scoped by a distributed lock, so two concurrent redeliveries of the same
//                  RiskScoredEvent could both observe "no existing CAPA" before either had committed
//                  its INSERT. Now wrapped in an IDistributedLockProvider lock keyed by RiskId (see
//                  the Eventing & Messaging Agent task's own worked example,
//                  "CrossModule:RiskCapa:{RiskId}") so the two redeliveries serialize instead of
//                  racing. The dedicated [System].[CrossModuleDedupKeys] ledger is intentionally NOT
//                  used here -- the real CorrectiveAction aggregate itself already carries the
//                  business key -- lock-scoping alone closes the remaining gap. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.RiskContinuousImprovement;

/// <summary>
/// Idempotent consumer that reacts to <see cref="RiskScoredEvent"/> by opening a real
/// ContinuousImprovement CAPA CorrectiveAction whenever the scored risk is High or Critical
/// (§3.3 direction 1: RiskManagement escalations generate CAPA/RCA tasks).
/// </summary>
public sealed class RiskEscalationCapaConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    ICorrectiveActionRepository correctiveActionRepository,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<RiskEscalationCapaConsumer> logger)
    : CrossModuleIdempotentConsumerBase<RiskScoredEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <summary>Maximum time to wait for the per-risk lock before giving up and letting MassTransit retry/dead-letter the message.</summary>
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    /// <summary>Discriminator recorded on the generated CAPA's SourceEventType, correlating it back to the originating Risk.</summary>
    public const string SourceEventType = "RiskManagement.Risk";

    /// <inheritdoc/>
    protected override Guid ResolveEventId(RiskScoredEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<RiskScoredEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        if (evt.Level != RiskLevel.High && evt.Level != RiskLevel.Critical)
        {
            Logger.LogDebug(
                "RiskEscalationCapaConsumer: risk {RiskId} scored {Level} (below High) -- no CAPA generated.",
                evt.RiskId, evt.Level.Code);
            return;
        }

        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "RiskEscalationCapaConsumer: no TenantId header on risk {RiskId}; skipping CAPA generation.",
                evt.RiskId);
            return;
        }

        // Defense-in-depth idempotency guard: the ProcessedEvents dedup check (base class) and this
        // CAPA creation are NOT part of the same database transaction (CreateAsync opens its own
        // connection/transaction), so a crash/redelivery between the two could otherwise create a
        // second CorrectiveAction for the same Risk on message replay. Detect that by business key
        // (TenantId + SourceEventId + SourceEventType) before creating. The whole check-then-create
        // sequence is scoped by a distributed lock keyed on RiskId so two concurrent redeliveries of
        // the same RiskScoredEvent cannot both observe "no existing CAPA" before either commits.
        await using var lockHandle = await lockProvider.AcquireAsync(
            $"CrossModule:RiskCapa:{evt.RiskId.Value}", LockTimeout, cancellationToken);

        var (existing, _) = await correctiveActionRepository.ListAsync(
            tenantId, sourceEventId: evt.RiskId.Value, actionType: null, priority: null, status: null,
            orderBy: "DueAtUtc", sortDirection: "ASC", pageNumber: 1, pageSize: 25, cancellationToken);

        if (existing.Any(ca => ca.SourceEventType == SourceEventType))
        {
            Logger.LogInformation(
                "RiskEscalationCapaConsumer: a CAPA already exists for risk {RiskId} (SourceEventType={SourceEventType}) -- skipping duplicate creation on replay.",
                evt.RiskId, SourceEventType);
            return;
        }

        var isCritical = evt.Level == RiskLevel.Critical;
        var caId = Guid.NewGuid();
        var dueAtUtc = evt.ScoredAtUtc.Value.AddDays(isCritical ? 3 : 14);

        await correctiveActionRepository.CreateAsync(
            caId: caId,
            tenantId: tenantId,
            sourceEventId: evt.RiskId.Value,
            sourceEventType: SourceEventType,
            title: $"CAPA for {evt.Level.DisplayName} hazard (Score {evt.Score.Score})",
            description: $"Corrective/preventive action required following {evt.Level.DisplayName} risk score " +
                          $"(Severity {evt.Score.Severity} x Likelihood {evt.Score.Likelihood} = {evt.Score.Score}) recorded for Risk {evt.RiskId}.",
            actionType: "Corrective",
            priority: isCritical ? "Critical" : "High",
            assignedToWorkerId: actorId,
            dueAtUtc: dueAtUtc,
            actorId: actorId,
            correlationId: evt.CorrelationId.Value,
            cancellationToken: cancellationToken);

        Logger.LogInformation(
            "RiskEscalationCapaConsumer: CAPA {CaId} opened for {Level} risk {RiskId} (due {DueAtUtc:u}).",
            caId, evt.Level.Code, evt.RiskId, dueAtUtc);
    }
}
