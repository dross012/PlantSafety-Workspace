// ============================================================================
// File: ComplianceFailureDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the ComplianceFailure module.
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

namespace PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Diagnostics;

/// <summary>Diagnostic constants for the ComplianceFailure module.</summary>
public static class ComplianceFailureDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for ComplianceFailure traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Predictive.ComplianceFailure";

    /// <summary>Histogram metric name for ComplianceFailure query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Predictive.ComplianceFailureLC_.ComplianceFailureLC_.query_duration_ms";

    /// <summary>Counter metric name for ComplianceFailure command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Predictive.ComplianceFailureLC_.ComplianceFailureLC_.command_duration_ms";
}