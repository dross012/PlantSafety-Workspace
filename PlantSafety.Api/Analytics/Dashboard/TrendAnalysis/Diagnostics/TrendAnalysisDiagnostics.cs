// ============================================================================
// File: TrendAnalysisDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the TrendAnalysis module.
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

namespace PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Diagnostics;

/// <summary>Diagnostic constants for the TrendAnalysis module.</summary>
public static class TrendAnalysisDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for TrendAnalysis traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Dashboard.TrendAnalysis";

    /// <summary>Histogram metric name for TrendAnalysis query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Dashboard.TrendAnalysisLC_.TrendAnalysisLC_.query_duration_ms";

    /// <summary>Counter metric name for TrendAnalysis command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Dashboard.TrendAnalysisLC_.TrendAnalysisLC_.command_duration_ms";
}