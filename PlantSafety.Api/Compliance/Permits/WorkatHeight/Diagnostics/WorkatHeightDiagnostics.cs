// ============================================================================
// File: WorkatHeightDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the WorkatHeight module.
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

namespace PlantSafety.Api.Compliance.Permits.WorkatHeight.Diagnostics;

/// <summary>Diagnostic constants for the WorkatHeight module.</summary>
public static class WorkatHeightDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for WorkatHeight traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.WorkatHeight";

    /// <summary>Histogram metric name for WorkatHeight query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.WorkatHeightLC_.WorkatHeightLC_.query_duration_ms";

    /// <summary>Counter metric name for WorkatHeight command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.WorkatHeightLC_.WorkatHeightLC_.command_duration_ms";
}