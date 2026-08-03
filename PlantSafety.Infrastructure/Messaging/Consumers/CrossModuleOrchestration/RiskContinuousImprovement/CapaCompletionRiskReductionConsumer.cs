// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.RiskContinuousImprovement
//  Layer:          Infrastructure
//  File:           CapaCompletionRiskReductionConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.3 RiskManagement <-> ContinuousImprovement)
//  SchemaVersion:  2026.1
//  Description:    Direction 2 of §3.3: when a CAPA CorrectiveAction completes work
//                  (ContinuousImprovement.CAPA.Events.CorrectiveActionCompletedEvent), resolve the
//                  originating Risk via the SourceEventId/SourceEventType recorded at CAPA-creation
//                  time (see RiskEscalationCapaConsumer, which tags
//                  SourceEventType = "RiskManagement.Risk") and dispatch the real
//                  ValidateRiskEffectivenessCommand so the completed improvement is reflected as a
//                  reduced residual risk score (Full Risk Management Process Phase 2, Step 7),
//                  closing the RiskManagement <-> ContinuousImprovement feedback loop.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): ValidateRiskEffectivenessCommand
//                  mutates the Risk aggregate's residual Severity/Likelihood by subtracting
//                  ResidualReductionSteps each time it runs (see the handler), so a redelivery that
//                  re-dispatches it would double-apply the residual risk reduction -- there is no
//                  natural idempotent-upsert shape here. Dedup by the completed CorrectiveAction's
//                  own CorrectiveActionId (the business key of "this specific CAPA completion has
//                  already been reflected as a risk reduction"), wrapped in TryExecuteOnceAsync and
//                  scoped by a matching distributed lock. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.RiskContinuousImprovement;

/// <summary>
/// Idempotent consumer that reacts to <see cref="CorrectiveActionCompletedEvent"/> by validating
/// (and reducing) the originating Risk's residual score via
/// <see cref="ValidateRiskEffectivenessCommand"/> (§3.3 direction 2: completed improvements reduce
/// future risk scores).
/// </summary>
public sealed class CapaCompletionRiskReductionConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    ICorrectiveActionRepository correctiveActionRepository,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<CapaCompletionRiskReductionConsumer> logger)
    : CrossModuleIdempotentConsumerBase<CorrectiveActionCompletedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    // A completed CAPA is modelled as reducing both Severity and Likelihood by one 5x5
    // risk-matrix band (floor of 1), a conservative, documented heuristic pending a dedicated
    // "effectiveness of implemented control" measurement pipeline.
    private const int ResidualReductionSteps = 1;

    /// <inheritdoc/>
    protected override Guid ResolveEventId(CorrectiveActionCompletedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<CorrectiveActionCompletedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "CapaCompletionRiskReductionConsumer: no TenantId header on CAPA {CaId}; skipping risk-reduction lookup.",
                evt.CorrectiveActionId);
            return;
        }

        var readModel = await correctiveActionRepository.GetByIdAsync(tenantId, evt.CorrectiveActionId.Value, cancellationToken);
        if (readModel is null)
        {
            Logger.LogWarning(
                "CapaCompletionRiskReductionConsumer: CAPA {CaId} not found for tenant {TenantId}; cannot resolve originating risk.",
                evt.CorrectiveActionId, tenantId);
            return;
        }

        if (!string.Equals(readModel.SourceEventType, RiskEscalationCapaConsumer.SourceEventType, StringComparison.Ordinal))
        {
            Logger.LogDebug(
                "CapaCompletionRiskReductionConsumer: CAPA {CaId} SourceEventType={SourceEventType} did not originate from RiskManagement -- no risk-effectiveness validation to perform.",
                evt.CorrectiveActionId, readModel.SourceEventType);
            return;
        }

        var correctiveActionId = evt.CorrectiveActionId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:CapaRiskReduction:{correctiveActionId}",
            businessKey: correctiveActionId.ToString(),
            eventId: evt.EventId.Value,
            sideEffect: async () =>
            {
                var command = new ValidateRiskEffectivenessCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: evt.CorrelationId.Value,
                    CausationId: evt.EventId.Value,
                    RequestedAtUtc: DateTime.UtcNow,
                    TenantId: tenantId,
                    RiskId: readModel.SourceEventId,
                    ResidualSeverity: ResidualReductionSteps,
                    ResidualLikelihood: ResidualReductionSteps,
                    ValidatedBy: actorId);

                await DispatchAsync(command,
                    $"ValidateRiskEffectivenessCommand for risk {readModel.SourceEventId} (CAPA {evt.CorrectiveActionId} completed)",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
