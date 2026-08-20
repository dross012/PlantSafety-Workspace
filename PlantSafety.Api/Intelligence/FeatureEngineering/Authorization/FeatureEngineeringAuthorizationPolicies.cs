// ============================================================================
// File: FeatureEngineeringAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code api-contract-validation-agent)
// Created: 2026-08-20
// Description: Authorization policy name constants for the Intelligence.FeatureEngineering
//              module. Mirrors PlantSafety.Api.Intelligence.DataLake.Authorization
//              .DataLakeAuthorizationPolicies. Auto-discovered and registered by
//              AuthorizationPolicyExtensions.AddPlantSafetyPolicies() -- no other file needs
//              changing when this class is added.
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
namespace PlantSafety.Api.Intelligence.FeatureEngineering.Authorization;

/// <summary>Authorization policy name constants for the Intelligence.FeatureEngineering module.</summary>
public static class FeatureEngineeringAuthorizationPolicies
{
    /// <summary>Read access to FeatureEngineering data. Granted to authorized roles.</summary>
    public const string Read = "RequireIntelligenceFeatureEngineeringRead";

    /// <summary>Write access to FeatureEngineering data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntelligenceFeatureEngineeringWrite";
}
