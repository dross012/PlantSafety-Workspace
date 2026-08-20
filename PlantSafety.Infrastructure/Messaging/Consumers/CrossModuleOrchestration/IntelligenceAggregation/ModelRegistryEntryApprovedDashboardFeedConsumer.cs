// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           ModelRegistryEntryApprovedDashboardFeedConsumer.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code new-features-agent)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 4: Intelligence.ModelRegistry's
//                  ModelRegistryEntryApprovedEvent (a human-approved model, per Safety Rule 4.5 --
//                  ApprovedByActorId must always be a human actor) is documented in the Intelligence
//                  design manual's Integration Patterns section as feeding "Model Registry Events for
//                  deployment and versioning", consumed by Intelligence.PredictiveSafety (which
//                  stamps ModelVersion onto every PredictiveSafetyAlert it raises -- see
//                  CreatePredictiveSafetyAlertCommand.ModelVersion) and Analytics (BI dashboards).
//                  Mirrors the exact, already-proven PredictiveSafetyAlertDashboardFeedConsumer shape:
//                  ingests a curated Intelligence.DataLake activity record rather than inventing a new
//                  cross-module write, since a model *approval* has no PredictiveSafety alert or
//                  Analytics aggregate of its own to mutate -- announcing the newly-approved
//                  model/version for downstream consumption is the correctly-scoped side effect.
//                  Additive-only evolution.
//
//                  Dedup rationale: CreateDataLakeActivityCommand always mints a new DataLakeActivity
//                  row (INSERT-only, no upsert key), so a redelivery must be hard-rejected rather than
//                  fed a second duplicate curated record for the same approval. Dedup'd by the event's
//                  own ModelRegistryEntryId+ModelVersion pair, wrapped in TryExecuteOnceAsync and
//                  scoped by a matching distributed lock.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="ModelRegistryEntryApprovedEvent"/> by ingesting a
/// curated Intelligence.DataLake activity record announcing the newly-approved model/version for
/// downstream Intelligence.PredictiveSafety and Analytics consumption (§3.4).
/// </summary>
public sealed class ModelRegistryEntryApprovedDashboardFeedConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<ModelRegistryEntryApprovedDashboardFeedConsumer> logger)
    : CrossModuleIdempotentConsumerBase<ModelRegistryEntryApprovedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(ModelRegistryEntryApprovedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<ModelRegistryEntryApprovedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        // ModelRegistryEntryApprovedEvent does not itself carry TenantId (only
        // ModelRegistryEntryRegisteredEvent does), so the propagated outbox envelope header is the
        // sole source here -- same fallback pattern as InsufficientMitigationInsightConsumer.
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = evt.ApprovedByActorId.Value != Guid.Empty ? evt.ApprovedByActorId.Value : context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "ModelRegistryEntryApprovedDashboardFeedConsumer: no TenantId available for model {ModelName} v{ModelVersion}; skipping PredictiveSafety/Analytics feed.",
                evt.ModelName, evt.ModelVersion);
            return;
        }

        var sourceEventId = evt.EventId.Value;
        var businessKey = $"{evt.ModelRegistryEntryId.Value}:{evt.ModelVersion}";

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:ModelRegistryEntryApproved:{businessKey}",
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
                    DatasetName: "Intelligence.PredictiveSafety.ModelRegistryApproved",
                    Zone: "Curated",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand PredictiveSafety/Analytics feed for approved model {evt.ModelName} v{evt.ModelVersion} ({evt.ModelRegistryEntryId})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
