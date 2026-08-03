// ============================================================================
// File: WorkflowAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Authorization policy name constants for the Workflow module (Phase 5 §5 — human +
//              machine workflow runtime).
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on WorkflowV1Controller.
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies() via
//   reflection over every "*AuthorizationPolicies" class — no additional registration required.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers.
//
// ============================================================================

namespace PlantSafety.Api.Workflow.Authorization;

/// <summary>Authorization policy name constants for the Workflow module.</summary>
public static class WorkflowAuthorizationPolicies
{
    /// <summary>Read access to Workflow instances/definitions/history. Granted to authorized roles.</summary>
    public const string Read = "RequireWorkflowRead";

    /// <summary>Write access to start/advance/retry/cancel a WorkflowInstance. Granted to editor roles.</summary>
    public const string Write = "RequireWorkflowWrite";

    /// <summary>Approval access to approve/reject/escalate a WorkflowInstance's current step. Granted to approver roles.</summary>
    public const string Approve = "RequireWorkflowApprove";
}
