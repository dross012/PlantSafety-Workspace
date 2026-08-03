// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           RecordCrossSiteActionCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Command recording a CrossSiteActionEvent into the GlobalSafetyInsights audit
//                  trail. Dispatched by CrossSiteActionConsumer -- a real, working shell for the
//                  Multi-Site Coordination workflow's commands (built by a later agent) to publish
//                  into. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Commands;

/// <summary>Records a cross-site coordinated action into the GlobalSafetyInsights audit trail.</summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="AffectedSiteIds">Sites affected by the action.</param>
/// <param name="ActionType">Short discriminator for the kind of action (e.g. "CorrectiveActionRollout").</param>
/// <param name="Description">Human-readable description of the action.</param>
/// <param name="InitiatedByUserId">User that initiated the action, if known.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering event.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering event.</param>
/// <param name="RegionCode">The region the action was resolved/routed in, if known.</param>
/// <param name="WorkflowInstanceId">The approval-chain WorkflowInstance gating this action, if one was started.</param>
public sealed record RecordCrossSiteActionCommand(
    Guid TenantId,
    IReadOnlyList<Guid> AffectedSiteIds,
    string ActionType,
    string Description,
    Guid? InitiatedByUserId,
    Guid CorrelationId,
    Guid CausationId,
    string? RegionCode = null,
    Guid? WorkflowInstanceId = null) : IRequest<Result>;
