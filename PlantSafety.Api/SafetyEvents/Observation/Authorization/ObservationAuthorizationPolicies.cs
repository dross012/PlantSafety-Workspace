// ============================================================================
// File: ObservationAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Observation module.
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

namespace PlantSafety.Api.SafetyEvents.Observation.Authorization;

/// <summary>Authorization policy name constants for the Observation module.</summary>
public static class ObservationAuthorizationPolicies
{
    /// <summary>Read access to Observation data. Granted to authorized roles.</summary>
    public const string Read  = "RequireSafetyEventsObservationRead";

    /// <summary>Write access to Observation data. Granted to administrative roles.</summary>
    public const string Write = "RequireSafetyEventsObservationWrite";
}