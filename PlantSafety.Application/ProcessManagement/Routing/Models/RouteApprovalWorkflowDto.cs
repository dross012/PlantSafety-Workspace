// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteApprovalWorkflowDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model projection of a RouteDefinition's approval-workflow state, derived
//                  from its reused ProcessManagement-style lifecycle Status. Shape matches
//                  PlantSafety.UI.Modules.Routing.Models.RoutingApprovalWorkflowDto/
//                  RoutingApproverDto field-for-field.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Single approval workflow stage/approver row.</summary>
public sealed record RouteApproverDto
{
    /// <summary>Approver display name.</summary>
    public required string ApproverName { get; init; }

    /// <summary>Approver's role.</summary>
    public required string Role { get; init; }

    /// <summary>Approver's decision status (Pending/Approved/Rejected).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the decision, if made.</summary>
    public DateTime? DecidedAtUtc { get; init; }
}

/// <summary>Full approval workflow state for a route definition.</summary>
public sealed record RouteApprovalWorkflowDto
{
    /// <summary>Current stage name (Draft/Review/Approved/Retired/Archived).</summary>
    public required string CurrentStage { get; init; }

    /// <summary>Assigned approvers for this workflow.</summary>
    public required IReadOnlyList<RouteApproverDto> Approvers { get; init; }

    /// <summary>Whether the current caller may approve/reject at this stage.</summary>
    public required bool CanCurrentUserAct { get; init; }
}
