// ============================================================================
// File: ErgonomicsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Ergonomics module.
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

namespace PlantSafety.Api.Operational.Ergonomics.Authorization;

/// <summary>Authorization policy name constants for the Ergonomics module.</summary>
public static class ErgonomicsAuthorizationPolicies
{
    /// <summary>Read access to Ergonomics data. Granted to authorized roles.</summary>
    public const string Read  = "RequireOperationalErgonomicsRead";

    /// <summary>Write access to Ergonomics data. Granted to administrative roles.</summary>
    public const string Write = "RequireOperationalErgonomicsWrite";
}