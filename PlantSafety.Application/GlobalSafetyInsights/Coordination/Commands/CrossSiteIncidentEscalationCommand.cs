// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           CrossSiteIncidentEscalationCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Dispatches a real, governed incident escalation to each affected site's real
//                  SafetyEvents.IncidentManagement command surface: the most recent open Incident
//                  at that site is escalated via the real EscalateIncidentCommand, or -- when no
//                  open incident exists at that site -- a new cross-site-flagged Incident is
//                  created via the real CreateIncidentCommand. Dispatched by MultiSiteActionRouter
//                  only via ISender (Handbook Part IV §4.2.2 Zero Cross-Module Leakage -- never
//                  reaches into SafetyEvents.IncidentManagement's Domain internals directly).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Commands;

/// <summary>Escalates (or creates) an Incident record at each of a set of affected sites, as part of a governed cross-site coordinated action.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="AffectedSiteIds">Sites the escalation must be routed to (already policy-eligible; see MultiSiteActionRouter).</param>
/// <param name="Reason">Human-readable reason for the escalation, recorded on every touched/created Incident.</param>
/// <param name="InitiatedByUserId">User that initiated the coordinated action, if human-triggered; <see langword="null"/> for a system-triggered (auto-escalated) action.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering anomaly/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering anomaly/request.</param>
public sealed record CrossSiteIncidentEscalationCommand(
    Guid TenantId,
    IReadOnlyList<Guid> AffectedSiteIds,
    string Reason,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId) : IRequest<Result<CrossSiteIncidentEscalationResult>>;

/// <summary>Per-site outcome of a <see cref="CrossSiteIncidentEscalationCommand"/>.</summary>
/// <param name="SiteId">The site this outcome pertains to.</param>
/// <param name="Success">Whether the dispatch to SafetyEvents.IncidentManagement succeeded for this site.</param>
/// <param name="IncidentId">The escalated or newly created Incident's identity, when <see cref="Success"/> is <see langword="true"/>.</param>
/// <param name="Action">"Escalated" when an existing open Incident was escalated, "Created" when a new cross-site-flagged Incident was created.</param>
/// <param name="ErrorMessage">Sanitized failure reason, when <see cref="Success"/> is <see langword="false"/>.</param>
public sealed record CrossSiteIncidentSiteOutcome(Guid SiteId, bool Success, Guid? IncidentId, string Action, string? ErrorMessage);

/// <summary>Result of a <see cref="CrossSiteIncidentEscalationCommand"/> execution.</summary>
/// <param name="Outcomes">One outcome per entry in <see cref="CrossSiteIncidentEscalationCommand.AffectedSiteIds"/>.</param>
public sealed record CrossSiteIncidentEscalationResult(IReadOnlyList<CrossSiteIncidentSiteOutcome> Outcomes)
{
    /// <summary><see langword="true"/> when every affected site was successfully escalated/created.</summary>
    public bool AllSucceeded => Outcomes.All(o => o.Success);
}
