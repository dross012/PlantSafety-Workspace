// ============================================================================
// File: HotTapDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the HotTap module.
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

namespace PlantSafety.Api.Compliance.Permits.HotTap.Diagnostics;

/// <summary>Diagnostic constants for the HotTap module.</summary>
public static class HotTapDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for HotTap traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.HotTap";

    /// <summary>Histogram metric name for HotTap query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.HotTapLC_.HotTapLC_.query_duration_ms";

    /// <summary>Counter metric name for HotTap command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.HotTapLC_.HotTapLC_.command_duration_ms";
}