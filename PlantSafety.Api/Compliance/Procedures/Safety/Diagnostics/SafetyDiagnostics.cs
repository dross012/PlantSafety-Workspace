// ============================================================================
// File: SafetyDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Safety module.
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

namespace PlantSafety.Api.Compliance.Procedures.Safety.Diagnostics;

/// <summary>Diagnostic constants for the Safety module.</summary>
public static class SafetyDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Safety traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.Safety";

    /// <summary>Histogram metric name for Safety query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.SafetyLC_.SafetyLC_.query_duration_ms";

    /// <summary>Counter metric name for Safety command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.SafetyLC_.SafetyLC_.command_duration_ms";
}