// ============================================================================
// File: IncidentResponseAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the IncidentResponse module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on controllers and AddPlantSafetyAuthorization() in ServiceCollectionExtensions.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers and DI registration.
//
// Future Extension Seams:
// - Add granular per-operation policies (e.g. RequireExport) if export endpoints are added.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.IncidentResponse.Authorization;

/// <summary>Authorization policy name constants for the IncidentResponse module.</summary>
public static class IncidentResponseAuthorizationPolicies
{
    /// <summary>Read access to IncidentResponse data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceProceduresIncidentResponseRead";

    /// <summary>Write access to IncidentResponse data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceProceduresIncidentResponseWrite";
}