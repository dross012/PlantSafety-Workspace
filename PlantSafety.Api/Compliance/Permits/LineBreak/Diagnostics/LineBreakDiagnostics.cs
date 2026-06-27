// ============================================================================
// File: LineBreakDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the LineBreak module.
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

namespace PlantSafety.Api.Compliance.Permits.LineBreak.Diagnostics;

/// <summary>Diagnostic constants for the LineBreak module.</summary>
public static class LineBreakDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for LineBreak traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.LineBreak";

    /// <summary>Histogram metric name for LineBreak query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.LineBreakLC_.LineBreakLC_.query_duration_ms";

    /// <summary>Counter metric name for LineBreak command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.LineBreakLC_.LineBreakLC_.command_duration_ms";
}