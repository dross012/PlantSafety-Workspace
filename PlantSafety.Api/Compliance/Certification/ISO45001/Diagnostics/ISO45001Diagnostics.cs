// ============================================================================
// File: ISO45001Diagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the ISO45001 module.
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

namespace PlantSafety.Api.Compliance.Certification.ISO45001.Diagnostics;

/// <summary>Diagnostic constants for the ISO45001 module.</summary>
public static class ISO45001Diagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for ISO45001 traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Certification.ISO45001";

    /// <summary>Histogram metric name for ISO45001 query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Certification.ISO45001LC_.ISO45001LC_.query_duration_ms";

    /// <summary>Counter metric name for ISO45001 command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Certification.ISO45001LC_.ISO45001LC_.command_duration_ms";
}