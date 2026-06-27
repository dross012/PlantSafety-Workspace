// ============================================================================
// File: RootCauseAnalysisDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the RootCauseAnalysis module.
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

namespace PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Diagnostics;

/// <summary>Diagnostic constants for the RootCauseAnalysis module.</summary>
public static class RootCauseAnalysisDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for RootCauseAnalysis traces.</summary>
    public const string ActivitySourceName = "PlantSafety.SafetyEvents.RootCauseAnalysis";

    /// <summary>Histogram metric name for RootCauseAnalysis query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.SafetyEvents.RootCauseAnalysisLC_.RootCauseAnalysisLC_.query_duration_ms";

    /// <summary>Counter metric name for RootCauseAnalysis command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.SafetyEvents.RootCauseAnalysisLC_.RootCauseAnalysisLC_.command_duration_ms";
}