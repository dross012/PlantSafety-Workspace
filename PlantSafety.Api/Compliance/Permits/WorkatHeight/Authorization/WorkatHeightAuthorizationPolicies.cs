// ============================================================================
// File: WorkatHeightAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the WorkatHeight module.
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

namespace PlantSafety.Api.Compliance.Permits.WorkatHeight.Authorization;

/// <summary>Authorization policy name constants for the WorkatHeight module.</summary>
public static class WorkatHeightAuthorizationPolicies
{
    /// <summary>Read access to WorkatHeight data. Granted to authorized roles.</summary>
    public const string Read  = "RequireCompliancePermitsWorkatHeightRead";

    /// <summary>Write access to WorkatHeight data. Granted to administrative roles.</summary>
    public const string Write = "RequireCompliancePermitsWorkatHeightWrite";
}