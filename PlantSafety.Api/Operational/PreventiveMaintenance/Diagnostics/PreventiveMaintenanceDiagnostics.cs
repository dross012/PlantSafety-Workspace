// ============================================================================
// File: PreventiveMaintenanceDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PreventiveMaintenance module.
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

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Diagnostics;

/// <summary>Diagnostic constants for the PreventiveMaintenance module.</summary>
public static class PreventiveMaintenanceDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PreventiveMaintenance traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.PreventiveMaintenance";

    /// <summary>Histogram metric name for PreventiveMaintenance query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.PreventiveMaintenanceLC_.PreventiveMaintenanceLC_.query_duration_ms";

    /// <summary>Counter metric name for PreventiveMaintenance command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.PreventiveMaintenanceLC_.PreventiveMaintenanceLC_.command_duration_ms";
}