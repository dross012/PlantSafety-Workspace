// ============================================================================
// File: EnvironmentalMonitoringDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: ActivitySource names and metric name constants for the EnvironmentalMonitoring module.
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

namespace PlantSafety.Api.Operational.EnvironmentalMonitoring.Diagnostics;

/// <summary>Diagnostic constants for the EnvironmentalMonitoring module.</summary>
public static class EnvironmentalMonitoringDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EnvironmentalMonitoring traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.EnvironmentalMonitoring";

    /// <summary>Histogram metric name for EnvironmentalMonitoring query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.EnvironmentalMonitoringLC_.EnvironmentalMonitoringLC_.query_duration_ms";

    /// <summary>Counter metric name for EnvironmentalMonitoring command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.EnvironmentalMonitoringLC_.EnvironmentalMonitoringLC_.command_duration_ms";
}