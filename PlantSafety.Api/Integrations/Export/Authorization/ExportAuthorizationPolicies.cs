// ============================================================================
// File: ExportAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Authorization policy name constants for the Integrations.Export module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on controllers and AddPlantSafetyAuthorization() in ServiceCollectionExtensions.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers and DI registration.
//
// Future Extension Seams:
// - Add granular per-operation policies (e.g. RequireExport) if additional endpoints are added.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Export.Authorization;

/// <summary>Authorization policy name constants for the Integrations.Export module.</summary>
public static class ExportAuthorizationPolicies
{
    /// <summary>Read access to Integrations.Export data. Granted to authorized roles.</summary>
    public const string Read  = "RequireIntegrationsExportRead";

    /// <summary>Write access to Integrations.Export data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntegrationsExportWrite";
}
