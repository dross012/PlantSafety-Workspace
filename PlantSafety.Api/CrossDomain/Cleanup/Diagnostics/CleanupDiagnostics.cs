// ============================================================================
// File: CleanupDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Cleanup module.
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

namespace PlantSafety.Api.CrossDomain.Cleanup.Diagnostics;

/// <summary>Diagnostic constants for the Cleanup module.</summary>
public static class CleanupDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Cleanup traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Cleanup";

    /// <summary>Histogram metric name for Cleanup query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.CleanupLC_.CleanupLC_.query_duration_ms";

    /// <summary>Counter metric name for Cleanup command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.CleanupLC_.CleanupLC_.command_duration_ms";
}