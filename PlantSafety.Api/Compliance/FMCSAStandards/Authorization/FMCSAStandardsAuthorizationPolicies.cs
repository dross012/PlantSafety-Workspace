// ============================================================================
// File: FMCSAStandardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.FMCSAStandards
//              sub-feature. Read-only reference data -- no Write policy exists because
//              nothing ever writes to it through the API. Mirrors
//              OSHAStandardsAuthorizationPolicies exactly.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.FMCSAStandards.Authorization;

/// <summary>Authorization policy name constants for the Compliance.FMCSAStandards sub-feature.</summary>
public static class FMCSAStandardsAuthorizationPolicies
{
    /// <summary>Read access to FMCSA standards, requirements, and change history. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceFMCSAStandardsRead";
}
