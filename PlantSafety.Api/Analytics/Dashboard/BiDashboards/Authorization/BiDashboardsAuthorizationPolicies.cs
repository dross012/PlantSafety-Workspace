// ============================================================================
// File: BiDashboardsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Authorization policy name constants for the BiDashboards sub-feature (Phase 5 §4.3).
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on BiDashboardsV1Controller.
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies() via
//   reflection over every "*AuthorizationPolicies" class — no additional registration required.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers.
//
// Future Extension Seams:
// - Add a Write policy if BiDashboards ever exposes mutation endpoints.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.BiDashboards.Authorization;

/// <summary>Authorization policy name constants for the BiDashboards sub-feature.</summary>
public static class BiDashboardsAuthorizationPolicies
{
    /// <summary>Read access to BI Dashboard KPI data. Granted to authorized roles.</summary>
    public const string Read = "RequireAnalyticsDashboardBiDashboardsRead";
}
