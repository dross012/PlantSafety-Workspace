// ============================================================================
// File: PredictiveAnalyticsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PredictiveAnalytics module.
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

namespace PlantSafety.Api.Analytics.PredictiveAnalytics.Diagnostics;

/// <summary>Diagnostic constants for the PredictiveAnalytics module.</summary>
public static class PredictiveAnalyticsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PredictiveAnalytics traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.PredictiveAnalytics";

    /// <summary>Histogram metric name for PredictiveAnalytics query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.PredictiveAnalyticsLC_.PredictiveAnalyticsLC_.query_duration_ms";

    /// <summary>Counter metric name for PredictiveAnalytics command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.PredictiveAnalyticsLC_.PredictiveAnalyticsLC_.command_duration_ms";
}