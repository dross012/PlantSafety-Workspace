// ============================================================================
// File: ConfinedSpaceDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the ConfinedSpace module.
// ============================================================================
//
// Onboarding Notes:
// - Activity source names are consumed by OpenTelemetry instrumentation in the Infrastructure layer.
//
// Rationale:
// - Separating diagnostic constants from controllers prevents magic strings and ensures consistent trace names.
//
// Future Extension Seams:
// - Add custom metric names when fine-grained OTel metrics are instrumented.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.ConfinedSpace.Diagnostics;

/// <summary>Diagnostic constants for the ConfinedSpace module.</summary>
public static class ConfinedSpaceDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for ConfinedSpace traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.ConfinedSpace";

    /// <summary>Histogram metric name for ConfinedSpace query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.ConfinedSpaceLC_.ConfinedSpaceLC_.query_duration_ms";

    /// <summary>Counter metric name for ConfinedSpace command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.ConfinedSpaceLC_.ConfinedSpaceLC_.command_duration_ms";
}