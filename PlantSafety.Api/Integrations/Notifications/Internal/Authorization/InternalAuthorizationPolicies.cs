// ============================================================================
// File: InternalAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Authorization policy name constants for the Internal module.
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
namespace PlantSafety.Api.Integrations.Notifications.Internal.Authorization;

/// <summary>Authorization policy name constants for the Internal module.</summary>
public static class InternalAuthorizationPolicies
{
    /// <summary>Read access to Internal data. Granted to authorized roles.</summary>
    public const string Read = "RequireIntegrationsNotificationsInternalRead";

    /// <summary>Write access to Internal data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntegrationsNotificationsInternalWrite";
}