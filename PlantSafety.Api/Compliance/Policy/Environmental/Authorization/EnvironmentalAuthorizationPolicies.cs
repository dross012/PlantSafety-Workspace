// ============================================================================
// File: EnvironmentalAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Environmental module.
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

namespace PlantSafety.Api.Compliance.Policy.Environmental.Authorization;

/// <summary>Authorization policy name constants for the Environmental module.</summary>
public static class EnvironmentalAuthorizationPolicies
{
    /// <summary>Read access to Environmental data. Granted to authorized roles.</summary>
    public const string Read  = "RequireCompliancePolicyEnvironmentalRead";

    /// <summary>Write access to Environmental data. Granted to administrative roles.</summary>
    public const string Write = "RequireCompliancePolicyEnvironmentalWrite";
}