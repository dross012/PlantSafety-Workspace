// ============================================================================
// File: OSHADiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the OSHA module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.OSHA.Diagnostics;

/// <summary>Diagnostic constants for the OSHA module.</summary>
public static class OSHADiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for OSHA traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.RegulatoryCalendar.Inspections.OSHA";

    /// <summary>Histogram metric name for OSHA query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Inspections.OSHALC_.OSHALC_.query_duration_ms";

    /// <summary>Counter metric name for OSHA command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Inspections.OSHALC_.OSHALC_.command_duration_ms";
}