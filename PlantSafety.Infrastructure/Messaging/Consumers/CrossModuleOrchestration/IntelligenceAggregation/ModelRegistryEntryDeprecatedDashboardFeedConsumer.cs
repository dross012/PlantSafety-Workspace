// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           ModelRegistryEntryDeprecatedDashboardFeedConsumer.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code new-features-agent)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 5: Intelligence.ModelRegistry's
//                  ModelRegistryEntryDeprecatedEvent (Approved -> Deprecated, typically superseded by
//                  a newer version) is documented in the Intelligence design manual's Integration
//                  Patterns section as feeding "Model Registry Events for deployment and versioning",
//                  consumed by Intelligence.PredictiveSafety and Analytics. Mirrors
//                  ModelRegistryEntryApprovedDashboardFeedConsumer's shape (see that file for the full
//                  rationale on why a curated DataLake feed -- not an invented direct mutation of
//                  PredictiveSafety/Analytics aggregates -- is the correctly-scoped side effect).
//                  Additive-only evolution.
//
//                  Dedup rationale: CreateDataLakeActivityCommand always mints a new DataLakeActivity
//                  row (INSERT-only, no upsert key), so a redelivery must be hard-rejected rather than
//                  fed a second duplicate curated record for the same deprecation. Dedup'd by the
//                  event's own ModelRegistryEntryId+ModelVersion pair, wrapped in TryExecuteOnceAsync
//                  and scoped by a matching distributed lock.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="ModelRegistryEntryDeprecatedEvent"/> by ingesting a
/// curated Intelligence.DataLake activity record announcing the deprecated model/version for
/// downstream Intelligence.PredictiveSafety and Analytics consumption (§3.4).
/// </summary>
public sealed class ModelRegistryEntryDeprecatedDashboardFeedConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<ModelRegistryEntryDeprecatedDashboardFeedConsumer> logger)
    : CrossModuleIdempotentConsumerBase<ModelRegistryEntryDeprecatedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(ModelRegistryEntryDeprecatedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<ModelRegistryEntryDeprecatedEvent> context,
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
                "ModelRegistryEntryDeprecatedDashboardFeedConsumer: no TenantId available for model {ModelName} v{ModelVersion}; skipping PredictiveSafety/Analytics feed.",
                evt.ModelName, evt.ModelVersion);
            return;
        }

        var sourceEventId = evt.EventId.Value;
        var businessKey = $"{evt.ModelRegistryEntryId.Value}:{evt.ModelVersion}";

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:ModelRegistryEntryDeprecated:{businessKey}",
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
                    DatasetName: "Intelligence.PredictiveSafety.ModelRegistryDeprecated",
                    Zone: "Curated",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand PredictiveSafety/Analytics feed for deprecated model {evt.ModelName} v{evt.ModelVersion} ({evt.ModelRegistryEntryId}): {evt.DeprecationReason}",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}
