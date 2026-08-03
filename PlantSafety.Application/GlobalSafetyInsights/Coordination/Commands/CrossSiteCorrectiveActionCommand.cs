// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteCorrectiveActionCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Dispatches a real Corrective/Preventive Action (CAPA) rollout to each affected
//                  site via the real ContinuousImprovement.CAPA CreateCorrectiveActionCommand.
//                  Dispatched by MultiSiteActionRouter only via ISender (Handbook Part IV §4.2.2
//                  Zero Cross-Module Leakage). Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

/// <summary>Rolls out a Corrective/Preventive Action at each of a set of affected sites, as part of a governed cross-site coordinated action.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="AffectedSiteIds">Sites the corrective action must be rolled out to (already policy-eligible; see MultiSiteActionRouter).</param>
/// <param name="Title">Short title for the corrective action. Max 200 characters.</param>
/// <param name="Description">Description of the corrective action to be performed at each site.</param>
/// <param name="Priority">Priority classification (Low, Medium, High, Critical).</param>
/// <param name="SourceEventId">Identifier of the triggering source event (e.g. the GlobalSafetyInsights snapshot).</param>
/// <param name="SourceEventType">Type discriminator of the triggering source event (e.g. "GlobalSafetyAnomaly").</param>
/// <param name="DueAtUtc">UTC instant the corrective action is due at every affected site.</param>
/// <param name="AssignedToWorkerId">Worker assigned to implement the corrective action at every affected site (a per-site placeholder assignment -- real per-site routing to a site's safety manager is a follow-on enhancement once a Site aggregate/roster exists).</param>
/// <param name="InitiatedByUserId">User that initiated the coordinated action, if human-triggered; <see langword="null"/> for a system-triggered (auto-escalated) action.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering anomaly/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering anomaly/request.</param>
public sealed record CrossSiteCorrectiveActionCommand(
    Guid TenantId,
    IReadOnlyList<Guid> AffectedSiteIds,
    string Title,
    string Description,
    string Priority,
    Guid SourceEventId,
    string SourceEventType,
    DateTimeOffset DueAtUtc,
    Guid AssignedToWorkerId,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId) : IRequest<Result<CrossSiteCorrectiveActionResult>>;

/// <summary>Per-site outcome of a <see cref="CrossSiteCorrectiveActionCommand"/>.</summary>
/// <param name="SiteId">The site this outcome pertains to.</param>
/// <param name="Success">Whether the dispatch to ContinuousImprovement.CAPA succeeded for this site.</param>
/// <param name="CorrectiveActionId">The newly created CorrectiveAction's identity, when <see cref="Success"/> is <see langword="true"/>.</param>
/// <param name="ErrorMessage">Sanitized failure reason, when <see cref="Success"/> is <see langword="false"/>.</param>
public sealed record CrossSiteCorrectiveActionSiteOutcome(Guid SiteId, bool Success, Guid? CorrectiveActionId, string? ErrorMessage);

/// <summary>Result of a <see cref="CrossSiteCorrectiveActionCommand"/> execution.</summary>
/// <param name="Outcomes">One outcome per entry in <see cref="CrossSiteCorrectiveActionCommand.AffectedSiteIds"/>.</param>
public sealed record CrossSiteCorrectiveActionResult(IReadOnlyList<CrossSiteCorrectiveActionSiteOutcome> Outcomes)
{
    /// <summary><see langword="true"/> when every affected site's corrective action was successfully created.</summary>
    public bool AllSucceeded => Outcomes.All(o => o.Success);
}
