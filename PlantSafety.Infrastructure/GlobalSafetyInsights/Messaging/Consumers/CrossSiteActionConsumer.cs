// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           CrossSiteActionConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Idempotent MassTransit consumer for CrossSiteActionEvent. Real, working shell:
//                  the commands that RAISE CrossSiteActionEvent belong to the Multi-Site
//                  Coordination workflow (built by a later agent) -- this consumer is ready to
//                  receive that event today and records it into the GlobalSafetyInsights audit
//                  ledger via RecordCrossSiteActionCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Consumers;

/// <summary>Idempotent consumer that records a CrossSiteActionEvent into the GlobalSafetyInsights audit ledger.</summary>
public sealed class CrossSiteActionConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<CrossSiteActionConsumer> logger)
    : CrossModuleIdempotentConsumerBase<CrossSiteActionEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(CrossSiteActionEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<CrossSiteActionEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        if (evt.TenantId == Guid.Empty)
        {
            Logger.LogWarning("CrossSiteActionConsumer: event {EventId} carries no TenantId; skipping.", evt.EventId);
            return;
        }

        var command = new RecordCrossSiteActionCommand(
            TenantId: evt.TenantId,
            AffectedSiteIds: evt.AffectedSiteIds,
            ActionType: evt.ActionType,
            Description: evt.Description,
            InitiatedByUserId: evt.InitiatedByUserId,
            CorrelationId: evt.CorrelationId.Value,
            CausationId: evt.CausationId.Value,
            RegionCode: string.IsNullOrEmpty(evt.RegionCode) ? null : evt.RegionCode);

        // RecordCrossSiteActionCommand returns the non-generic Result (void command), which does
        // not fit CrossModuleIdempotentConsumerBase.DispatchAsync<TResponse>'s Result<TResponse>
        // constraint -- dispatched directly here instead, with the same success/failure logging
        // shape DispatchAsync applies to every other cross-module orchestration consumer.
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            Logger.LogInformation("CrossSiteActionConsumer: recorded '{ActionType}' for Tenant {TenantId} succeeded.", evt.ActionType, evt.TenantId);
        else
            Logger.LogWarning(
                "CrossSiteActionConsumer: recorded '{ActionType}' for Tenant {TenantId} failed: [{ErrorCode}] {Message}",
                evt.ActionType, evt.TenantId, result.Error.ErrorCode, result.Error.Message);
    }
}
