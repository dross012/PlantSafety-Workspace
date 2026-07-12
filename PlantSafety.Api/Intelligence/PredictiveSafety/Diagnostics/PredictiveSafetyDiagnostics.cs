// ============================================================================
// File: PredictiveSafetyDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the PredictiveSafety module.
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
namespace PlantSafety.Api.Intelligence.PredictiveSafety.Diagnostics;

/// <summary>Diagnostic constants for the PredictiveSafety module.</summary>
public static class PredictiveSafetyDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PredictiveSafety traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Intelligence.PredictiveSafety";

    /// <summary>Histogram metric name for PredictiveSafety query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Intelligence.PredictiveSafetyLC_.PredictiveSafetyLC_.query_duration_ms";

    /// <summary>Counter metric name for PredictiveSafety command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Intelligence.PredictiveSafetyLC_.PredictiveSafetyLC_.command_duration_ms";
}