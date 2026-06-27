// ============================================================================
// File: EngagementAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Authorization policy name constants for the Engagement Analytics read endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on controllers and AddPlantSafetyAuthorization() in ServiceCollectionExtensions.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers and DI registration.
//
// Future Extension Seams:
// - Add granular per-operation policies (e.g. RequireAnalyticsEngagementExport) if export endpoints are added.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Authorization;

/// <summary>Authorization policy name constants for the Engagement Analytics module.</summary>
public static class EngagementAuthorizationPolicies
{
    /// <summary>Read access to Engagement analytics data. Granted to dashboard and reporting roles.</summary>
    public const string Read   = "RequireAnalyticsEngagementRead";

    /// <summary>Export access for Engagement analytics reports.</summary>
    public const string Export = "RequireAnalyticsEngagementExport";
}