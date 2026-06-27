// ============================================================================
// File: ElectricalAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Electrical module.
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

namespace PlantSafety.Api.Compliance.Permits.Electrical.Authorization;

/// <summary>Authorization policy name constants for the Electrical module.</summary>
public static class ElectricalAuthorizationPolicies
{
    /// <summary>Read access to Electrical data. Granted to authorized roles.</summary>
    public const string Read  = "RequireCompliancePermitsElectricalRead";

    /// <summary>Write access to Electrical data. Granted to administrative roles.</summary>
    public const string Write = "RequireCompliancePermitsElectricalWrite";
}