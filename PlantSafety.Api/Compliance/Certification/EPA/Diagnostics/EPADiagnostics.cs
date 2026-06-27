// ============================================================================
// File: EPADiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the EPA module.
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

namespace PlantSafety.Api.Compliance.Certification.EPA.Diagnostics;

/// <summary>Diagnostic constants for the EPA module.</summary>
public static class EPADiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EPA traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Certification.EPA";

    /// <summary>Histogram metric name for EPA query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Certification.EPALC_.EPALC_.query_duration_ms";

    /// <summary>Counter metric name for EPA command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Certification.EPALC_.EPALC_.command_duration_ms";
}