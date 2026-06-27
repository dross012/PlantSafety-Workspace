// ============================================================================
// File: ExecutiveDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Executive module.
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

namespace PlantSafety.Api.Analytics.Dashboard.Executive.Diagnostics;

/// <summary>Diagnostic constants for the Executive module.</summary>
public static class ExecutiveDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Executive traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Dashboard.Executive";

    /// <summary>Histogram metric name for Executive query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Dashboard.ExecutiveLC_.ExecutiveLC_.query_duration_ms";

    /// <summary>Counter metric name for Executive command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Dashboard.ExecutiveLC_.ExecutiveLC_.command_duration_ms";
}