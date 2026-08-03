// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           GlobalSafetyAnomalyEscalationReactor.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Reacts to the GlobalSafetyAnomalyDetectedEvent(s) already raised on a freshly
//                  computed GlobalSafetyInsightsAggregate (Phase 8 §2 -- see
//                  GlobalSafetyInsightsAggregate.Compute). GlobalSafetyAnomalyDetectedEvent is a
//                  pure in-memory Domain event (not published to the MassTransit bus -- only the
//                  dashboard-facing GlobalSafetyAnomalyEvent is, via IGlobalSafetyInsightsPublisher),
//                  so the real, honest integration point is in-process: this reactor is invoked
//                  directly by ComputeGlobalSafetyInsightsCommandHandler immediately after the
//                  aggregate is computed, evaluates each anomaly against
//                  IMultiSiteSafetyPolicy.ShouldAutoEscalate, and for every anomaly that crosses the
//                  auto-escalation threshold, routes a CrossSiteIncidentEscalationCommand through
//                  MultiSiteActionRouter (validate -> policy -> route -> audit). Additive-only
//                  evolution.
// ============================================================================
using Microsoft.Extensions.Logging;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Models;
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination;

/// <summary>Reacts to a freshly computed snapshot's detected anomalies, auto-escalating the ones that cross the policy's severity threshold.</summary>
public interface IGlobalSafetyAnomalyEscalationReactor
{
    /// <summary>Evaluates every anomaly on <paramref name="aggregate"/> and routes an auto-escalation for each that crosses the threshold.</summary>
    Task ReactAsync(GlobalSafetyInsightsAggregate aggregate, Guid correlationId, Guid causationId, CancellationToken cancellationToken);
}

/// <inheritdoc cref="IGlobalSafetyAnomalyEscalationReactor"/>
public sealed class GlobalSafetyAnomalyEscalationReactor(
    IMultiSiteSafetyPolicy policy,
    IMultiSiteActionRouter router,
    ILogger<GlobalSafetyAnomalyEscalationReactor> logger)
    : IGlobalSafetyAnomalyEscalationReactor
{
    /// <inheritdoc/>
    public async Task ReactAsync(GlobalSafetyInsightsAggregate aggregate, Guid correlationId, Guid causationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        foreach (var anomaly in aggregate.Anomalies)
        {
            if (!policy.ShouldAutoEscalate(anomaly.DeviationMagnitude))
                continue;

            IReadOnlyList<Guid> candidateSiteIds = anomaly.SiteId.HasValue ? [anomaly.SiteId.Value] : aggregate.SiteIds;

            var request = new MultiSiteActionRequest(
                aggregate.TenantId,
                candidateSiteIds,
                CrossSiteActionType.IncidentEscalation,
                Description: $"Auto-escalated: {anomaly.Kind} (deviation magnitude {anomaly.DeviationMagnitude:F2}) -- {anomaly.Message}",
                TenantExcludedSiteIds: null,
                InitiatedByUserId: null,
                CorrelationId: correlationId,
                CausationId: causationId,
                IncidentEscalationReason: anomaly.Message);

            var result = await router.RouteAsync(request, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "GlobalSafetyAnomalyEscalationReactor: auto-escalation routing failed for {Kind} on Tenant {TenantId}: [{ErrorCode}] {Message}",
                    anomaly.Kind, aggregate.TenantId, result.Error.ErrorCode, result.Error.Message);
            }
            else
            {
                logger.LogInformation(
                    "GlobalSafetyAnomalyEscalationReactor: auto-escalated {Kind} on Tenant {TenantId} to {SiteCount} eligible site(s) (RequiresApproval={RequiresApproval}).",
                    anomaly.Kind, aggregate.TenantId, result.Value.EligibleSiteIds.Count, result.Value.RequiresApproval);
            }
        }
    }
}
