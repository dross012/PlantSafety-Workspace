// ============================================================================
// File: NIOSHStandardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.NIOSHStandards
//              sub-feature. Read-only reference data -- no Write policy exists because nothing
//              ever writes to it through the API (only the NIOSH reference-data sync tool does).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.NIOSHStandards.Authorization;

/// <summary>Authorization policy name constants for the Compliance.NIOSHStandards sub-feature.</summary>
public static class NIOSHStandardsAuthorizationPolicies
{
    /// <summary>Read access to NIOSH hazard profiles, respirator recommendations, PPE guidance, and change history. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceNIOSHStandardsRead";
}
