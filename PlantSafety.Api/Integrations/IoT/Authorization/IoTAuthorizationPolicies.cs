// ============================================================================
// File: IoTAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Authorization policy name constants for the Integrations.IoT module.
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

namespace PlantSafety.Api.Integrations.IoT.Authorization;

/// <summary>Authorization policy name constants for the Integrations.IoT module.</summary>
public static class IoTAuthorizationPolicies
{
    /// <summary>Read access to Integrations.IoT data. Granted to authorized roles.</summary>
    public const string Read  = "RequireIntegrationsIoTRead";

    /// <summary>Write access to Integrations.IoT data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntegrationsIoTWrite";
}
