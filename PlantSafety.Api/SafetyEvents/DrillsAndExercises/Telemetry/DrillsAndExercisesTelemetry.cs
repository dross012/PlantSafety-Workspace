// ============================================================================
// File: DrillsAndExercisesTelemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Telemetry span name and OpenTelemetry attribute key constants for the DrillsAndExercises module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as DrillsAndExercises telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for DrillsAndExercises.</summary>
public static class DrillsAndExercisesTelemetry
{
    /// <summary>Span name for a DrillsAndExercises read query.</summary>
    public const string QuerySpan = "SafetyEvents.DrillsAndExercisesLC_.DrillsAndExercisesLC_.query";

    /// <summary>Span name for a DrillsAndExercises write command.</summary>
    public const string CommandSpan = "SafetyEvents.DrillsAndExercisesLC_.DrillsAndExercisesLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.SafetyEvents.DrillsAndExercisesLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.SafetyEvents.DrillsAndExercisesLC_.cache_hit";
}