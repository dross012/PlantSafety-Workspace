// ============================================================================
// File: EnvironmentalDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Environmental module.
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

namespace PlantSafety.Api.Compliance.Policy.Environmental.Diagnostics;

/// <summary>Diagnostic constants for the Environmental module.</summary>
public static class EnvironmentalDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Environmental traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.Environmental";

    /// <summary>Histogram metric name for Environmental query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.EnvironmentalLC_.EnvironmentalLC_.query_duration_ms";

    /// <summary>Counter metric name for Environmental command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.EnvironmentalLC_.EnvironmentalLC_.command_duration_ms";
}