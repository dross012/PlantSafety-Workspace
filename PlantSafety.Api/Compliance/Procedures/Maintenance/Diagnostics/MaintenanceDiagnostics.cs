// ============================================================================
// File: MaintenanceDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Maintenance module.
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

namespace PlantSafety.Api.Compliance.Procedures.Maintenance.Diagnostics;

/// <summary>Diagnostic constants for the Maintenance module.</summary>
public static class MaintenanceDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Maintenance traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.Maintenance";

    /// <summary>Histogram metric name for Maintenance query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.MaintenanceLC_.MaintenanceLC_.query_duration_ms";

    /// <summary>Counter metric name for Maintenance command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.MaintenanceLC_.MaintenanceLC_.command_duration_ms";
}