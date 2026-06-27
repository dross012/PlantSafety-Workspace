// ============================================================================
// File: ObservationDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Observation module.
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

namespace PlantSafety.Api.SafetyEvents.Observation.Diagnostics;

/// <summary>Diagnostic constants for the Observation module.</summary>
public static class ObservationDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Observation traces.</summary>
    public const string ActivitySourceName = "PlantSafety.SafetyEvents.Observation";

    /// <summary>Histogram metric name for Observation query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.SafetyEvents.ObservationLC_.ObservationLC_.query_duration_ms";

    /// <summary>Counter metric name for Observation command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.SafetyEvents.ObservationLC_.ObservationLC_.command_duration_ms";
}