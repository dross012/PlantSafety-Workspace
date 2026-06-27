// ============================================================================
// File: SupervisorAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Supervisor module.
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

namespace PlantSafety.Api.Analytics.ScoreCards.Supervisor.Authorization;

/// <summary>Authorization policy name constants for the Supervisor module.</summary>
public static class SupervisorAuthorizationPolicies
{
    /// <summary>Read access to Supervisor data. Granted to authorized roles.</summary>
    public const string Read  = "RequireAnalyticsScoreCardsSupervisorRead";

    /// <summary>Write access to Supervisor data. Granted to administrative roles.</summary>
    public const string Write = "RequireAnalyticsScoreCardsSupervisorWrite";
}