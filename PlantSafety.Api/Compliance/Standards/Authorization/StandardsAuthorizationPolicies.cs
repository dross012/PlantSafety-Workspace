// ============================================================================
// File: StandardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.Standards
//              (ANSI/ASME/ISO consensus-standards) reference-data sub-feature. Read-only
//              reference data -- no Write policy exists because nothing ever writes to it
//              through the API. Mirrors EpaAuthorizationPolicies.cs's precedent.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Standards.Authorization;

/// <summary>Authorization policy name constants for the Compliance.Standards reference-data sub-feature.</summary>
public static class StandardsAuthorizationPolicies
{
    /// <summary>Read access to consensus-standards families, series, requirements, equipment requirements, and risk assessment rules. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceStandardsRead";
}
