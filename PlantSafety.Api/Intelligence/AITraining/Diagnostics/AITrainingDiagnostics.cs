// ============================================================================
// File: AITrainingDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the AITraining module.
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
namespace PlantSafety.Api.Intelligence.AITraining.Diagnostics;

/// <summary>Diagnostic constants for the AITraining module.</summary>
public static class AITrainingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for AITraining traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Intelligence.AITraining";

    /// <summary>Histogram metric name for AITraining query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Intelligence.AITrainingLC_.AITrainingLC_.query_duration_ms";

    /// <summary>Counter metric name for AITraining command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Intelligence.AITrainingLC_.AITrainingLC_.command_duration_ms";
}