// ============================================================================
// File: OSHAStandardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.OSHAStandards
//              sub-feature. Read-only reference data -- no Write policy exists because
//              nothing ever writes to it through the API (only tools/OshaStandardsImport does).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.OSHAStandards.Authorization;

/// <summary>Authorization policy name constants for the Compliance.OSHAStandards sub-feature.</summary>
public static class OSHAStandardsAuthorizationPolicies
{
    /// <summary>Read access to OSHA standards, requirements, and change history. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceOSHAStandardsRead";
}
