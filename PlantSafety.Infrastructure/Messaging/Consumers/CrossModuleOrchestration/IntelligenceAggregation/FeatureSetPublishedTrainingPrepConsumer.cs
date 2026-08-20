// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           FeatureSetPublishedTrainingPrepConsumer.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code new-features-agent)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 3: Intelligence.FeatureEngineering's
//                  FeatureSetPublishedEvent (Feature Engineering §5.4 -- feature pipelines and
//                  reuse across models) is documented as "triggers downstream training preparation"
//                  (consumed by Intelligence.AITraining) and, per the Intelligence design manual's
//                  Integration Patterns section, feeds the platform's "Feature Store Contracts for
//                  cross-module reuse" and "Model Registry Events for deployment and versioning"
//                  surfaces. Mirrors the exact, already-proven PredictiveSafetyAlertDashboardFeedConsumer
//                  / SchedulingKpiFeedbackConsumer shape: ingests a curated Intelligence.DataLake
//                  activity record tagged for the AITraining pipeline's consumption, rather than
//                  auto-initiating an actual CreateAITrainingJobCommand -- FeatureSetPublishedEvent
//                  carries no ModelName/ModelVersion/Hyperparameters, so guessing which model(s) to
//                  retrain from a feature-set publish alone would be an invented business decision,
//                  not a documented one. Deliberately conservative: this consumer only announces
//                  "a feature set is now available for training preparation"; the actual training
//                  job initiation remains a human/orchestrator decision via the existing
//                  CreateAITrainingJobCommand API surface. Additive-only evolution.
//
//                  Dedup rationale: CreateDataLakeActivityCommand always mints a new DataLakeActivity
//                  row (INSERT-only, no upsert key -- see DataLakeActivityAggregateDapperRepository.
//                  AddAsync), so a redelivery must be hard-rejected rather than allowed to feed a
//                  second duplicate curated record for the same published feature set. Dedup'd by the
//                  event's own FeatureSetId+Version pair, wrapped in TryExecuteOnceAsync and scoped by
//                  a matching distributed lock.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="FeatureSetPublishedEvent"/> by ingesting a curated
/// Intelligence.DataLake activity record announcing the published feature set's availability for
/// downstream Intelligence.AITraining preparation (§3.4: Intelligence-internal cross-module reuse).
/// </summary>
public sealed class FeatureSetPublishedTrainingPrepConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<FeatureSetPublishedTrainingPrepConsumer> logger)
    : CrossModuleIdempotentConsumerBase<FeatureSetPublishedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(FeatureSetPublishedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<FeatureSetPublishedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        // FeatureSetPublishedEvent does not itself carry TenantId (only FeatureSetCreatedEvent
        // does), so the propagated outbox envelope header is the sole source here -- same fallback
        // pattern as InsufficientMitigationInsightConsumer for RiskEffectivenessValidatedEvent.
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = evt.Actor.UserId?.Value ?? context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "FeatureSetPublishedTrainingPrepConsumer: no TenantId available for feature set {FeatureSetId} v{Version}; skipping AITraining feed.",
                evt.FeatureSetId, evt.Version);
            return;
        }

        var sourceEventId = evt.EventId.Value;
        var businessKey = $"{evt.FeatureSetId.Value}:{evt.Version}";

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:FeatureSetPublishedTrainingPrep:{businessKey}",
            businessKey: businessKey,
            eventId: sourceEventId,
            sideEffect: async () =>
            {
                var command = new CreateDataLakeActivityCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: evt.CorrelationId.Value,
                    CausationId: evt.EventId.Value,
                    RequestedAtUtc: DateTime.UtcNow,
                    ApiVersionToken: "v1",
                    TenantId: tenantId,
                    SiteId: null,
                    DatasetName: "Intelligence.AITraining.FeatureSetAvailable",
                    Zone: "ML",
                    RecordCount: evt.FeatureCount,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand training-prep feed for published feature set {evt.Name} v{evt.Version} ({evt.FeatureSetId})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
