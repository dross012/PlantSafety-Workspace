// ============================================================================
// File: LiftingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Lifting module.
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

namespace PlantSafety.Api.Compliance.Permits.Lifting.Diagnostics;

/// <summary>Diagnostic constants for the Lifting module.</summary>
public static class LiftingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Lifting traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.Lifting";

    /// <summary>Histogram metric name for Lifting query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.LiftingLC_.LiftingLC_.query_duration_ms";

    /// <summary>Counter metric name for Lifting command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.LiftingLC_.LiftingLC_.command_duration_ms";
}