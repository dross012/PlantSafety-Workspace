// ============================================================================
// File: AnalyticsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Authorization policy name constants for the Integrations.Analytics module.
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

namespace PlantSafety.Api.Integrations.Analytics.Authorization;

/// <summary>Authorization policy name constants for the Integrations.Analytics module.</summary>
public static class AnalyticsAuthorizationPolicies
{
    /// <summary>Read access to Integrations.Analytics data. Granted to authorized roles.</summary>
    public const string Read  = "RequireIntegrationsAnalyticsRead";

    /// <summary>Write access to Integrations.Analytics data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntegrationsAnalyticsWrite";
}
