// ============================================================================
// File: PredictiveSafetyAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Authorization policy name constants for the PredictiveSafety module.
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
namespace PlantSafety.Api.Intelligence.PredictiveSafety.Authorization;

/// <summary>Authorization policy name constants for the PredictiveSafety module.</summary>
public static class PredictiveSafetyAuthorizationPolicies
{
    /// <summary>Read access to PredictiveSafety data. Granted to authorized roles.</summary>
    public const string Read = "RequireIntelligencePredictiveSafetyRead";

    /// <summary>Write access to PredictiveSafety data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntelligencePredictiveSafetyWrite";
}