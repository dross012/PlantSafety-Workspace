// ============================================================================
// File: NFPAStandardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.NFPAStandards
//              sub-feature. Read-only reference data -- no Write policy exists because
//              nothing ever writes to it through the API (only tools/NfpaReferenceImport does).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.NFPAStandards.Authorization;

/// <summary>Authorization policy name constants for the Compliance.NFPAStandards sub-feature.</summary>
public static class NFPAStandardsAuthorizationPolicies
{
    /// <summary>Read access to NFPA hazard profiles, controls, and change history. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceNFPAStandardsRead";
}
