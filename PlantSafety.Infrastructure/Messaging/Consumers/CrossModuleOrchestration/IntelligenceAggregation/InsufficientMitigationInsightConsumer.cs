// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           InsufficientMitigationInsightConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.4 Intelligence <-> everything)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 1: Intelligence consumes RiskManagement's
//                  RiskEffectivenessValidatedEvent (Full Risk Management Process Phase 2, Step 7)
//                  and, when the validated residual risk is still NOT acceptable, generates a real
//                  Intelligence.PredictiveSafety alert via CreatePredictiveSafetyAlertCommand --
//                  turning a cross-module domain fact into a genuine predictive-model-facing
//                  insight, per "Intelligence should act as the aggregation point ... generating
//                  insights, feeding the predictive models".
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): CreatePredictiveSafetyAlertCommand
//                  always mints a new PredictiveSafetyAlert aggregate (INSERT-only, no upsert key),
//                  so a redelivery must be rejected rather than allowed to raise a second alert for
//                  the same validated risk. Dedup by the source RiskEffectivenessValidatedEvent's own
//                  EventId, wrapped in TryExecuteOnceAsync and scoped by a matching distributed lock.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.PredictiveSafety.Commands;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="RiskEffectivenessValidatedEvent"/> by raising a
/// real Intelligence.PredictiveSafety alert whenever residual risk validation fails
/// (§3.4: Intelligence aggregates cross-module facts into predictive-model-facing insights).
/// </summary>
public sealed class InsufficientMitigationInsightConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<InsufficientMitigationInsightConsumer> logger)
    : CrossModuleIdempotentConsumerBase<RiskEffectivenessValidatedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    private const string RiskCategory = "InsufficientMitigation";

    /// <inheritdoc/>
    protected override Guid ResolveEventId(RiskEffectivenessValidatedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<RiskEffectivenessValidatedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        if (evt.IsAcceptable)
        {
            Logger.LogDebug(
                "InsufficientMitigationInsightConsumer: risk {RiskId} validated as acceptable -- no predictive-safety insight generated.",
                evt.RiskId);
            return;
        }

        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "InsufficientMitigationInsightConsumer: no TenantId header on risk {RiskId}; skipping predictive-safety alert.",
                evt.RiskId);
            return;
        }

        // Normalises the residual risk-matrix score (max 25) onto the platform's canonical
        // 0.0-1.0 predicted-risk-score scale so it composes with the other predictive models that
        // feed Intelligence.PredictiveSafety (e.g. FailureLikelihoodPredictor's 0.0-1.0 output).
        var normalizedScore = Math.Clamp(evt.ResidualScore.Score / 25.0, 0.0, 1.0);

        var sourceEventId = evt.EventId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:InsufficientMitigation:{sourceEventId}",
            businessKey: sourceEventId.ToString(),
            eventId: sourceEventId,
            sideEffect: async () =>
            {
                var command = new CreatePredictiveSafetyAlertCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: evt.CorrelationId.Value,
                    CausationId: evt.EventId.Value,
                    RequestedAtUtc: DateTime.UtcNow,
                    ApiVersionToken: "v1",
                    TenantId: tenantId,
                    SiteId: null,
                    RiskCategory: RiskCategory,
                    PredictedRiskScore: normalizedScore,
                    ModelVersion: "RiskEffectivenessValidation-2026.1",
                    GeneratedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreatePredictiveSafetyAlertCommand for insufficiently-mitigated risk {evt.RiskId}",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
