// ============================================================================
// File: IncidentResponseDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the IncidentResponse module.
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

namespace PlantSafety.Api.Compliance.Procedures.IncidentResponse.Diagnostics;

/// <summary>Diagnostic constants for the IncidentResponse module.</summary>
public static class IncidentResponseDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for IncidentResponse traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.IncidentResponse";

    /// <summary>Histogram metric name for IncidentResponse query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.IncidentResponseLC_.IncidentResponseLC_.query_duration_ms";

    /// <summary>Counter metric name for IncidentResponse command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.IncidentResponseLC_.IncidentResponseLC_.command_duration_ms";
}