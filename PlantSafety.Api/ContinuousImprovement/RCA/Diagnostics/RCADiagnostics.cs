// ============================================================================
// File: RCADiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the RCA module.
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

namespace PlantSafety.Api.ContinuousImprovement.RCA.Diagnostics;

/// <summary>Diagnostic constants for the RCA module.</summary>
public static class RCADiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for RCA traces.</summary>
    public const string ActivitySourceName = "PlantSafety.ContinuousImprovement.RCA";

    /// <summary>Histogram metric name for RCA query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.ContinuousImprovement.RCALC_.RCALC_.query_duration_ms";

    /// <summary>Counter metric name for RCA command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.ContinuousImprovement.RCALC_.RCALC_.command_duration_ms";
}