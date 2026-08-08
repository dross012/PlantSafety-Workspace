// ============================================================================
// File: EpaAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Authorization policy name constants for the Compliance.EPA reference-data
//              sub-feature. Read-only reference data -- no Write policy exists because
//              nothing ever writes to it through the API. Uses the "Epa" prefix (not bare
//              "EPA") to avoid collision with the pre-existing, unrelated
//              Compliance.Certification.EPA / Compliance.RegulatoryCalendar.Deadlines.EPA
//              features and their EPAAuthorizationPolicies classes.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Compliance.EPA.Authorization;

/// <summary>Authorization policy name constants for the Compliance.EPA reference-data sub-feature.</summary>
public static class EpaAuthorizationPolicies
{
    /// <summary>Read access to EPA programs, requirements, threshold/reporting/recordkeeping rules. Granted to authorized roles.</summary>
    public const string Read = "RequireComplianceEPARead";
}
