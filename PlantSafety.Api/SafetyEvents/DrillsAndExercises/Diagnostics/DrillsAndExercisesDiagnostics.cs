// ============================================================================
// File: DrillsAndExercisesDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the DrillsAndExercises module.
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

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Diagnostics;

/// <summary>Diagnostic constants for the DrillsAndExercises module.</summary>
public static class DrillsAndExercisesDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for DrillsAndExercises traces.</summary>
    public const string ActivitySourceName = "PlantSafety.SafetyEvents.DrillsAndExercises";

    /// <summary>Histogram metric name for DrillsAndExercises query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.SafetyEvents.DrillsAndExercisesLC_.DrillsAndExercisesLC_.query_duration_ms";

    /// <summary>Counter metric name for DrillsAndExercises command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.SafetyEvents.DrillsAndExercisesLC_.DrillsAndExercisesLC_.command_duration_ms";
}