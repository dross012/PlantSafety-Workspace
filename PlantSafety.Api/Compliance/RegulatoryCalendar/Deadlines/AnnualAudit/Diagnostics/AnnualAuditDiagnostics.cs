// ============================================================================
// File: AnnualAuditDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the AnnualAudit module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Diagnostics;

/// <summary>Diagnostic constants for the AnnualAudit module.</summary>
public static class AnnualAuditDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for AnnualAudit traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit";

    /// <summary>Histogram metric name for AnnualAudit query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Deadlines.AnnualAuditLC_.AnnualAuditLC_.query_duration_ms";

    /// <summary>Counter metric name for AnnualAudit command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Deadlines.AnnualAuditLC_.AnnualAuditLC_.command_duration_ms";
}