// ============================================================================
// File: EmergencyProcedureDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the EmergencyProcedure module.
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

namespace PlantSafety.Api.Compliance.Procedures.EmergencyProcedure.Diagnostics;

/// <summary>Diagnostic constants for the EmergencyProcedure module.</summary>
public static class EmergencyProcedureDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EmergencyProcedure traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.EmergencyProcedure";

    /// <summary>Histogram metric name for EmergencyProcedure query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.EmergencyProcedureLC_.EmergencyProcedureLC_.query_duration_ms";

    /// <summary>Counter metric name for EmergencyProcedure command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.EmergencyProcedureLC_.EmergencyProcedureLC_.command_duration_ms";
}