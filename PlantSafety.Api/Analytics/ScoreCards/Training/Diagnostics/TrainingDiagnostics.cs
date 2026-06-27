// ============================================================================
// File: TrainingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Training module.
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

namespace PlantSafety.Api.Analytics.ScoreCards.Training.Diagnostics;

/// <summary>Diagnostic constants for the Training module.</summary>
public static class TrainingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Training traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.ScoreCards.Training";

    /// <summary>Histogram metric name for Training query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.ScoreCards.TrainingLC_.TrainingLC_.query_duration_ms";

    /// <summary>Counter metric name for Training command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.ScoreCards.TrainingLC_.TrainingLC_.command_duration_ms";
}