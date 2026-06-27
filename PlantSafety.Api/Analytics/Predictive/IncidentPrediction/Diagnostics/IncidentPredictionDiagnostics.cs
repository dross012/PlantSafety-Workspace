// ============================================================================
// File: IncidentPredictionDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the IncidentPrediction module.
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

namespace PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Diagnostics;

/// <summary>Diagnostic constants for the IncidentPrediction module.</summary>
public static class IncidentPredictionDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for IncidentPrediction traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Predictive.IncidentPrediction";

    /// <summary>Histogram metric name for IncidentPrediction query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Predictive.IncidentPredictionLC_.IncidentPredictionLC_.query_duration_ms";

    /// <summary>Counter metric name for IncidentPrediction command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Predictive.IncidentPredictionLC_.IncidentPredictionLC_.command_duration_ms";
}