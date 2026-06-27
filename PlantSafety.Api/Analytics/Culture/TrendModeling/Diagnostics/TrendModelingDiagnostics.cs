// ============================================================================
// File: TrendModelingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the TrendModeling module.
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

namespace PlantSafety.Api.Analytics.Culture.TrendModeling.Diagnostics;

/// <summary>Diagnostic constants for the TrendModeling module.</summary>
public static class TrendModelingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for TrendModeling traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Culture.TrendModeling";

    /// <summary>Histogram metric name for TrendModeling query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Culture.TrendModelingLC_.TrendModelingLC_.query_duration_ms";

    /// <summary>Counter metric name for TrendModeling command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Culture.TrendModelingLC_.TrendModelingLC_.command_duration_ms";
}