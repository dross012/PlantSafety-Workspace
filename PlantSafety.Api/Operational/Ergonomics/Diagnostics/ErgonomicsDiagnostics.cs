// ============================================================================
// File: ErgonomicsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Ergonomics module.
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

namespace PlantSafety.Api.Operational.Ergonomics.Diagnostics;

/// <summary>Diagnostic constants for the Ergonomics module.</summary>
public static class ErgonomicsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Ergonomics traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.Ergonomics";

    /// <summary>Histogram metric name for Ergonomics query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.ErgonomicsLC_.ErgonomicsLC_.query_duration_ms";

    /// <summary>Counter metric name for Ergonomics command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.ErgonomicsLC_.ErgonomicsLC_.command_duration_ms";
}