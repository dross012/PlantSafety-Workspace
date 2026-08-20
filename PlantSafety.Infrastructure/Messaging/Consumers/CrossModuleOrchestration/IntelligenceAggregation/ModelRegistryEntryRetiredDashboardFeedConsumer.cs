// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           ModelRegistryEntryRetiredDashboardFeedConsumer.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code new-features-agent)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 6: Intelligence.ModelRegistry's
//                  ModelRegistryEntryRetiredEvent (terminal; no further transitions) is documented in
//                  the Intelligence design manual's Integration Patterns section as feeding "Model
//                  Registry Events for deployment and versioning", consumed by
//                  Intelligence.PredictiveSafety and Analytics. Mirrors
//                  ModelRegistryEntryApprovedDashboardFeedConsumer's shape (see that file for the full
//                  rationale on why a curated DataLake feed is the correctly-scoped side effect).
//                  ModelRegistryEntryRetiredEvent carries no ModelName/ModelVersion (unlike Approved/
//                  Deprecated), so the ModelRegistryEntryId alone is used as the business/dedup key
//                  and dataset annotation -- retirement is terminal, so at most one such record will
//                  ever exist per entry. Additive-only evolution.
//
//                  Dedup rationale: CreateDataLakeActivityCommand always mints a new DataLakeActivity
//                  row (INSERT-only, no upsert key), so a redelivery must be hard-rejected rather than
//                  fed a second duplicate curated record for the same retirement. Dedup'd by the
//                  event's own ModelRegistryEntryId, wrapped in TryExecuteOnceAsync and scoped by a
//                  matching distributed lock.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="ModelRegistryEntryRetiredEvent"/> by ingesting a
/// curated Intelligence.DataLake activity record announcing the retired model registry entry for
/// downstream Intelligence.PredictiveSafety and Analytics consumption (§3.4).
/// </summary>
public sealed class ModelRegistryEntryRetiredDashboardFeedConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<ModelRegistryEntryRetiredDashboardFeedConsumer> logger)
    : CrossModuleIdempotentConsumerBase<ModelRegistryEntryRetiredEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(ModelRegistryEntryRetiredEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<ModelRegistryEntryRetiredEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        var tenantId = context.GetEnvelopeTenantId();
        var actorId = evt.Actor.UserId?.Value ?? context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "ModelRegistryEntryRetiredDashboardFeedConsumer: no TenantId available for model registry entry {ModelRegistryEntryId}; skipping PredictiveSafety/Analytics feed.",
                evt.ModelRegistryEntryId);
            return;
        }

        var sourceEventId = evt.EventId.Value;
        var businessKey = evt.ModelRegistryEntryId.Value.ToString();

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:ModelRegistryEntryRetired:{businessKey}",
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
                    DatasetName: "Intelligence.PredictiveSafety.ModelRegistryRetired",
                    Zone: "Curated",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand PredictiveSafety/Analytics feed for retired model registry entry {evt.ModelRegistryEntryId}",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
