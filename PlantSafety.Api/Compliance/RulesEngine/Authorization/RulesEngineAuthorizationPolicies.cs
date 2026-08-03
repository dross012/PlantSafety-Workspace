// ============================================================================
// File: RulesEngineAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Authorization policy name constants for the Compliance.RulesEngine sub-feature
//              (Phase 5 §6.3).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RulesEngine.Authorization;

/// <summary>Authorization policy name constants for the Compliance.RulesEngine sub-feature.</summary>
public static class RulesEngineAuthorizationPolicies
{
    /// <summary>Read access to ComplianceEvaluation runs and their history. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceRulesEngineRead";

    /// <summary>Access to trigger a new ComplianceEvaluation run. Granted to editor roles.</summary>
    public const string Write = "RequireComplianceRulesEngineWrite";
}
