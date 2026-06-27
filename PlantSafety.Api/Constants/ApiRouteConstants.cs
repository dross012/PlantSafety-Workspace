// ============================================================================
// File: ApiRouteConstants.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: String constants for all API route prefixes and segment fragments
//              used across PlantSafety.Api controllers and minimal-API endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Controller RouteAttribute values should reference these constants via
//   interpolated strings (e.g. $"api/v{{version:apiVersion}}/{ApiRouteConstants.Analytics}").
//   This prevents the versioned prefix from diverging across 121 controllers.
// - VersionedBase is the canonical route prefix template. The {version:apiVersion}
//   placeholder is resolved by Asp.Versioning.Mvc at request time.
// - Module route segments use kebab-case to follow REST conventions. They match
//   the route prefixes registered in gen_analytics.ps1 / gen_compliance.ps1 and
//   gen_modules.ps1.
//
// Rationale:
// - Centralising route segments means renaming a module route is a one-line change
//   here rather than a find/replace across all controller files.
//
// Future Extension Seams:
// - Add v3 prefix constant if a V3 generation is planned.
// - Add sub-resource segments (e.g. "attachments", "comments") if nested resource
//   routing is adopted.
//
// ============================================================================

namespace PlantSafety.Api.Constants;

/// <summary>String constants for all route segments in PlantSafety.Api.</summary>
public static class ApiRouteConstants
{
    /// <summary>
    /// The canonical versioned API route prefix.
    /// Use as the base in <see cref="Microsoft.AspNetCore.Mvc.RouteAttribute"/>.
    /// </summary>
    public const string VersionedBase = "api/v{version:apiVersion}";

    // ── Module route segments ─────────────────────────────────────────────────

    /// <summary>Route segment for the Analytics bounded context.</summary>
    public const string Analytics = "analytics";

    /// <summary>Route segment for the Compliance bounded context.</summary>
    public const string Compliance = "compliance";

    /// <summary>Route segment for the Continuous Improvement bounded context.</summary>
    public const string ContinuousImprovement = "continuous-improvement";

    /// <summary>Route segment for the Integrations bounded context.</summary>
    public const string Integrations = "integrations";

    /// <summary>Route segment for the Intelligence bounded context.</summary>
    public const string Intelligence = "intelligence";

    /// <summary>Route segment for the Operational bounded context.</summary>
    public const string Operational = "operational";

    /// <summary>Route segment for the Safety Events bounded context.</summary>
    public const string SafetyEvents = "safety-events";

    // ── Common sub-resource segments ──────────────────────────────────────────

    /// <summary>Sub-resource route segment for export operations (e.g. .../export).</summary>
    public const string Export = "export";

    /// <summary>Sub-resource route segment for approval operations.</summary>
    public const string Approve = "approve";

    /// <summary>Sub-resource route segment for bulk operations.</summary>
    public const string Bulk = "bulk";

    // ── Health and meta routes ────────────────────────────────────────────────

    /// <summary>Root health-check path. Mapped in ApplicationBuilderExtensions.</summary>
    public const string Health = "/health";

    /// <summary>Readiness health-check path.</summary>
    public const string HealthReady = "/health/ready";

    /// <summary>Liveness health-check path.</summary>
    public const string HealthLive = "/health/live";
}
