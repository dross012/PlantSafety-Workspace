// ============================================================================
// File: EmergencyDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Emergency module.
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

namespace PlantSafety.Api.Compliance.Policy.Emergency.Diagnostics;

/// <summary>Diagnostic constants for the Emergency module.</summary>
public static class EmergencyDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Emergency traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.Emergency";

    /// <summary>Histogram metric name for Emergency query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.EmergencyLC_.EmergencyLC_.query_duration_ms";

    /// <summary>Counter metric name for Emergency command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.EmergencyLC_.EmergencyLC_.command_duration_ms";
}