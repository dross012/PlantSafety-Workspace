// ============================================================================
// File: EnvironmentalSamplingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the EnvironmentalSampling module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Diagnostics;

/// <summary>Diagnostic constants for the EnvironmentalSampling module.</summary>
public static class EnvironmentalSamplingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EnvironmentalSampling traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling";

    /// <summary>Histogram metric name for EnvironmentalSampling query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSamplingLC_.EnvironmentalSamplingLC_.query_duration_ms";

    /// <summary>Counter metric name for EnvironmentalSampling command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSamplingLC_.EnvironmentalSamplingLC_.command_duration_ms";
}