// ============================================================================
// File: WorkforceSafetyAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Authorization policy name constants for the WorkforceSafety module.
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

namespace PlantSafety.Api.Operational.WorkforceSafety.Authorization;

/// <summary>Authorization policy name constants for the WorkforceSafety module.</summary>
public static class WorkforceSafetyAuthorizationPolicies
{
    /// <summary>Read access to WorkforceSafety data. Granted to authorized roles.</summary>
    public const string Read  = "RequireOperationalWorkforceSafetyRead";

    /// <summary>Write access to WorkforceSafety data. Granted to administrative roles.</summary>
    public const string Write = "RequireOperationalWorkforceSafetyWrite";
}