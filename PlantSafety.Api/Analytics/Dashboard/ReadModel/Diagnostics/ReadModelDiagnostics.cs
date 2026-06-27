// ============================================================================
// File: ReadModelDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the ReadModel module.
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

namespace PlantSafety.Api.Analytics.Dashboard.ReadModel.Diagnostics;

/// <summary>Diagnostic constants for the ReadModel module.</summary>
public static class ReadModelDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for ReadModel traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Dashboard.ReadModel";

    /// <summary>Histogram metric name for ReadModel query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Dashboard.ReadModelLC_.ReadModelLC_.query_duration_ms";

    /// <summary>Counter metric name for ReadModel command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Dashboard.ReadModelLC_.ReadModelLC_.command_duration_ms";
}