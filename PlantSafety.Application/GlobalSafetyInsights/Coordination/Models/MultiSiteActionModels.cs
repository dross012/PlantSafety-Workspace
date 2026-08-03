// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Application
//  File:           MultiSiteActionModels.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Request/result models for MultiSiteActionRouter.RouteAsync. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;

namespace PlantSafety.Application.GlobalSafetyInsights.Coordination.Models;

/// <summary>Request to route a governed coordinated action across a set of candidate sites.</summary>
/// <param name="TenantId">Tenant issuing the coordinated action.</param>
/// <param name="CandidateSiteIds">Sites proposed to participate in the action.</param>
/// <param name="ActionType">The kind of coordinated action.</param>
/// <param name="Description">Human-readable description of the action, recorded to the audit trail and (if applicable) surfaced on the approval workflow.</param>
/// <param name="TenantExcludedSiteIds">Sites the tenant has explicitly excluded from cross-site coordination, if configured.</param>
/// <param name="InitiatedByUserId">User that initiated the coordinated action, if human-triggered; <see langword="null"/> for a system-triggered (auto-escalated) action.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering anomaly/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering anomaly/request.</param>
/// <param name="IncidentEscalationReason">Reason used when <paramref name="ActionType"/> is <see cref="CrossSiteActionType.IncidentEscalation"/>.</param>
/// <param name="LockoutPropagationReason">Reason used when <paramref name="ActionType"/> is <see cref="CrossSiteActionType.LockoutPropagation"/>.</param>
/// <param name="CorrectiveAction">Payload used when <paramref name="ActionType"/> is <see cref="CrossSiteActionType.CorrectiveActionRollout"/>.</param>
/// <param name="MaintenanceCoordination">Payload used when <paramref name="ActionType"/> is <see cref="CrossSiteActionType.MaintenanceCoordination"/>.</param>
public sealed record MultiSiteActionRequest(
    Guid TenantId,
    IReadOnlyList<Guid> CandidateSiteIds,
    CrossSiteActionType ActionType,
    string Description,
    IReadOnlyList<Guid>? TenantExcludedSiteIds,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId,
    string? IncidentEscalationReason = null,
    string? LockoutPropagationReason = null,
    CrossSiteCorrectiveActionPayload? CorrectiveAction = null,
    CrossSiteMaintenanceCoordinationPayload? MaintenanceCoordination = null);

/// <summary>Payload required to dispatch a <see cref="CrossSiteActionType.CorrectiveActionRollout"/> action.</summary>
/// <param name="Title">Short title for the corrective action.</param>
/// <param name="Priority">Priority classification.</param>
/// <param name="SourceEventId">Identifier of the triggering source event.</param>
/// <param name="SourceEventType">Type discriminator of the triggering source event.</param>
/// <param name="DueAtUtc">UTC instant the corrective action is due.</param>
/// <param name="AssignedToWorkerId">Worker assigned to implement the corrective action.</param>
public sealed record CrossSiteCorrectiveActionPayload(
    string Title,
    string Priority,
    Guid SourceEventId,
    string SourceEventType,
    DateTimeOffset DueAtUtc,
    Guid AssignedToWorkerId);

/// <summary>Payload required to dispatch a <see cref="CrossSiteActionType.MaintenanceCoordination"/> action.</summary>
/// <param name="MachineId">Identifier of the targeted machine/equipment family.</param>
/// <param name="MachineCode">Human-readable machine code.</param>
/// <param name="Priority">Priority classification.</param>
/// <param name="DueAtUtc">UTC instant the work order is due.</param>
public sealed record CrossSiteMaintenanceCoordinationPayload(
    Guid MachineId,
    string MachineCode,
    string Priority,
    DateTime DueAtUtc);

/// <summary>Outcome of <see cref="MultiSiteActionRouter.RouteAsync"/>.</summary>
/// <param name="EligibleSiteIds">Sites the action was (or, pending approval, will be) routed to.</param>
/// <param name="IneligibleSites">Candidate sites excluded by policy, with reasons.</param>
/// <param name="RequiresApproval">Whether the action required an approval chain before it could execute.</param>
/// <param name="Executed">
/// <see langword="true"/> when the action's site-level commands were dispatched immediately;
/// <see langword="false"/> when the action is pending approval (a WorkflowInstance was started, or
/// -- if no approval WorkflowDefinition is configured for the tenant -- the action was recorded as
/// pending-approval without automatic execution).
/// </param>
/// <param name="DispatchSucceeded">Whether the dispatch to the target site-level module(s) succeeded, when <see cref="Executed"/> is <see langword="true"/>.</param>
/// <param name="DispatchErrorMessage">Sanitized dispatch failure reason, when <see cref="DispatchSucceeded"/> is <see langword="false"/>.</param>
/// <param name="WorkflowInstanceId">The started approval WorkflowInstance's identity, when an approval WorkflowDefinition was found and <see cref="RequiresApproval"/> is <see langword="true"/>.</param>
public sealed record MultiSiteActionRouteResult(
    IReadOnlyList<Guid> EligibleSiteIds,
    IReadOnlyList<IneligibleSite> IneligibleSites,
    bool RequiresApproval,
    bool Executed,
    bool DispatchSucceeded,
    string? DispatchErrorMessage,
    Guid? WorkflowInstanceId);
