// ============================================================================
// File: SOPsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the SOPs module.
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

namespace PlantSafety.Api.Compliance.Procedures.SOPs.Diagnostics;

/// <summary>Diagnostic constants for the SOPs module.</summary>
public static class SOPsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for SOPs traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.SOPs";

    /// <summary>Histogram metric name for SOPs query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.SOPsLC_.SOPsLC_.query_duration_ms";

    /// <summary>Counter metric name for SOPs command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.SOPsLC_.SOPsLC_.command_duration_ms";
}