// ============================================================================
// File: WebinarsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Webinars module.
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

namespace PlantSafety.Api.Operational.Training.Webinars.Diagnostics;

/// <summary>Diagnostic constants for the Webinars module.</summary>
public static class WebinarsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Webinars traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.Training.Webinars";

    /// <summary>Histogram metric name for Webinars query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.Training.WebinarsLC_.WebinarsLC_.query_duration_ms";

    /// <summary>Counter metric name for Webinars command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.Training.WebinarsLC_.WebinarsLC_.command_duration_ms";
}