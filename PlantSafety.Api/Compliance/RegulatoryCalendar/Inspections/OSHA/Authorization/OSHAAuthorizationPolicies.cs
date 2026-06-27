// ============================================================================
// File: OSHAAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the OSHA module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.OSHA.Authorization;

/// <summary>Authorization policy name constants for the OSHA module.</summary>
public static class OSHAAuthorizationPolicies
{
    /// <summary>Read access to OSHA data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceRegCalOSHARead";

    /// <summary>Write access to OSHA data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceRegCalOSHAWrite";
}