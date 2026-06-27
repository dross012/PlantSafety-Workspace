// ============================================================================
// File: InternalSafetyAuditDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the InternalSafetyAudit module.
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

namespace PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Diagnostics;

/// <summary>Diagnostic constants for the InternalSafetyAudit module.</summary>
public static class InternalSafetyAuditDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for InternalSafetyAudit traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Auditing.InternalSafetyAudit";

    /// <summary>Histogram metric name for InternalSafetyAudit query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Auditing.InternalSafetyAuditLC_.InternalSafetyAuditLC_.query_duration_ms";

    /// <summary>Counter metric name for InternalSafetyAudit command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Auditing.InternalSafetyAuditLC_.InternalSafetyAuditLC_.command_duration_ms";
}