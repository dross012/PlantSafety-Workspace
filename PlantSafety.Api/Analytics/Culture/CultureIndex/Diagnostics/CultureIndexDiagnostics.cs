// ============================================================================
// File: CultureIndexDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the CultureIndex module.
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

namespace PlantSafety.Api.Analytics.Culture.CultureIndex.Diagnostics;

/// <summary>Diagnostic constants for the CultureIndex module.</summary>
public static class CultureIndexDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for CultureIndex traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Culture.CultureIndex";

    /// <summary>Histogram metric name for CultureIndex query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Culture.CultureIndexLC_.CultureIndexLC_.query_duration_ms";

    /// <summary>Counter metric name for CultureIndex command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Culture.CultureIndexLC_.CultureIndexLC_.command_duration_ms";
}