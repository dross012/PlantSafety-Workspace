// ============================================================================
// File: AlertsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Alerts module.
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

namespace PlantSafety.Api.Operational.Alerts.Diagnostics;

/// <summary>Diagnostic constants for the Alerts module.</summary>
public static class AlertsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Alerts traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.Alerts";

    /// <summary>Histogram metric name for Alerts query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.AlertsLC_.AlertsLC_.query_duration_ms";

    /// <summary>Counter metric name for Alerts command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.AlertsLC_.AlertsLC_.command_duration_ms";
}