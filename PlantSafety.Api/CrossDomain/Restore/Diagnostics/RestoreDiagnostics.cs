// ============================================================================
// File: RestoreDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Restore module.
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

namespace PlantSafety.Api.CrossDomain.Restore.Diagnostics;

/// <summary>Diagnostic constants for the Restore module.</summary>
public static class RestoreDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Restore traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Restore";

    /// <summary>Histogram metric name for Restore query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.RestoreLC_.RestoreLC_.query_duration_ms";

    /// <summary>Counter metric name for Restore command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.RestoreLC_.RestoreLC_.command_duration_ms";
}