// ============================================================================
// File: ModelRegistryAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code api-contract-validation-agent)
// Created: 2026-08-20
// Description: Authorization policy name constants for the Intelligence.ModelRegistry module.
//              Mirrors PlantSafety.Api.Intelligence.DataLake.Authorization
//              .DataLakeAuthorizationPolicies. Includes a dedicated Approve tier (mapped by
//              AuthorizationPolicyExtensions.TierForFieldName's "Approve" case to
//              SafetyRoles.ApproverAndAbove) because ApproveModelCommand is SAFETY-CRITICAL
//              (Safety Rule 4.5) and must require a human approver role, distinct from the
//              general Write tier used by the other lifecycle transitions. Auto-discovered and
//              registered by AuthorizationPolicyExtensions.AddPlantSafetyPolicies() -- no other
//              file needs changing when this class is added.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on controllers. Registered automatically via
//   reflection in Extensions/AuthorizationPolicyExtensions.cs (AddPlantSafetyPolicies()).
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.ModelRegistry.Authorization;

/// <summary>Authorization policy name constants for the Intelligence.ModelRegistry module.</summary>
public static class ModelRegistryAuthorizationPolicies
{
    /// <summary>Read access to ModelRegistry data. Granted to authorized roles.</summary>
    public const string Read = "RequireIntelligenceModelRegistryRead";

    /// <summary>Write access to ModelRegistry data (register/submit/deprecate/retire). Granted to administrative roles.</summary>
    public const string Write = "RequireIntelligenceModelRegistryWrite";

    /// <summary>
    /// Approval access for the SAFETY-CRITICAL PendingApproval -&gt; Approved transition (Safety
    /// Rule 4.5). Granted only to human approver roles (<c>SafetyRoles.ApproverAndAbove</c>).
    /// </summary>
    public const string Approve = "RequireIntelligenceModelRegistryApprove";
}
