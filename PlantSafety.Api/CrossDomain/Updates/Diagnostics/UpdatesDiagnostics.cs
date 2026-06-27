// ============================================================================
// File: UpdatesDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Updates module.
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

namespace PlantSafety.Api.CrossDomain.Updates.Diagnostics;

/// <summary>Diagnostic constants for the Updates module.</summary>
public static class UpdatesDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Updates traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Updates";

    /// <summary>Histogram metric name for Updates query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.UpdatesLC_.UpdatesLC_.query_duration_ms";

    /// <summary>Counter metric name for Updates command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.UpdatesLC_.UpdatesLC_.command_duration_ms";
}