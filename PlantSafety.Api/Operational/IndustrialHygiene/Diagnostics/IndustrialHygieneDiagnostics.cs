// ============================================================================
// File: IndustrialHygieneDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the IndustrialHygiene module.
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

namespace PlantSafety.Api.Operational.IndustrialHygiene.Diagnostics;

/// <summary>Diagnostic constants for the IndustrialHygiene module.</summary>
public static class IndustrialHygieneDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for IndustrialHygiene traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.IndustrialHygiene";

    /// <summary>Histogram metric name for IndustrialHygiene query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.IndustrialHygieneLC_.IndustrialHygieneLC_.query_duration_ms";

    /// <summary>Counter metric name for IndustrialHygiene command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.IndustrialHygieneLC_.IndustrialHygieneLC_.command_duration_ms";
}