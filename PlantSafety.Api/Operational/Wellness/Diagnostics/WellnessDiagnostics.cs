// ============================================================================
// File: WellnessDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: ActivitySource names and metric name constants for the Wellness module.
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

namespace PlantSafety.Api.Operational.Wellness.Diagnostics;

/// <summary>Diagnostic constants for the Wellness module.</summary>
public static class WellnessDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Wellness traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.Wellness";

    /// <summary>Histogram metric name for Wellness query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.WellnessLC_.WellnessLC_.query_duration_ms";

    /// <summary>Counter metric name for Wellness command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.WellnessLC_.WellnessLC_.command_duration_ms";
}