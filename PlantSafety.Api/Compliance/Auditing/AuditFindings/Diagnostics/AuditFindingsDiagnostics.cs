// ============================================================================
// File: AuditFindingsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the AuditFindings module.
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

namespace PlantSafety.Api.Compliance.Auditing.AuditFindings.Diagnostics;

/// <summary>Diagnostic constants for the AuditFindings module.</summary>
public static class AuditFindingsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for AuditFindings traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Auditing.AuditFindings";

    /// <summary>Histogram metric name for AuditFindings query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Auditing.AuditFindingsLC_.AuditFindingsLC_.query_duration_ms";

    /// <summary>Counter metric name for AuditFindings command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Auditing.AuditFindingsLC_.AuditFindingsLC_.command_duration_ms";
}