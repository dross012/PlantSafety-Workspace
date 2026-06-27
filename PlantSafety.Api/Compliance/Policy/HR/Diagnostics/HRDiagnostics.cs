// ============================================================================
// File: HRDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the HR module.
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

namespace PlantSafety.Api.Compliance.Policy.HR.Diagnostics;

/// <summary>Diagnostic constants for the HR module.</summary>
public static class HRDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for HR traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.HR";

    /// <summary>Histogram metric name for HR query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.HRLC_.HRLC_.query_duration_ms";

    /// <summary>Counter metric name for HR command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.HRLC_.HRLC_.command_duration_ms";
}