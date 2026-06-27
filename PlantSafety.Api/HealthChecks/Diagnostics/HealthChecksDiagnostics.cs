// ============================================================================
// File: HealthChecksDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the HealthChecks module.
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

namespace PlantSafety.Api.HealthChecks.Diagnostics;

/// <summary>Diagnostic constants for the HealthChecks module.</summary>
public static class HealthChecksDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for HealthChecks traces.</summary>
    public const string ActivitySourceName = "PlantSafety.HealthChecks";

    /// <summary>Histogram metric name for HealthChecks query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.HealthChecksLC_.HealthChecksLC_.query_duration_ms";

    /// <summary>Counter metric name for HealthChecks command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.HealthChecksLC_.HealthChecksLC_.command_duration_ms";
}