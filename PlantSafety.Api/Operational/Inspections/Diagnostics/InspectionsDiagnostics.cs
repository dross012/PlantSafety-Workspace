// ============================================================================
// File: InspectionsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Inspections module.
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

namespace PlantSafety.Api.Operational.Inspections.Diagnostics;

/// <summary>Diagnostic constants for the Inspections module.</summary>
public static class InspectionsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Inspections traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.Inspections";

    /// <summary>Histogram metric name for Inspections query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.InspectionsLC_.InspectionsLC_.query_duration_ms";

    /// <summary>Counter metric name for Inspections command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.InspectionsLC_.InspectionsLC_.command_duration_ms";
}