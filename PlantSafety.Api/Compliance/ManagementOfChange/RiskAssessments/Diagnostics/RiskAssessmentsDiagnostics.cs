// ============================================================================
// File: RiskAssessmentsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the RiskAssessments module.
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

namespace PlantSafety.Api.Compliance.ManagementOfChange.RiskAssessments.Diagnostics;

/// <summary>Diagnostic constants for the RiskAssessments module.</summary>
public static class RiskAssessmentsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for RiskAssessments traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.ManagementOfChange.RiskAssessments";

    /// <summary>Histogram metric name for RiskAssessments query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.ManagementOfChange.RiskAssessmentsLC_.RiskAssessmentsLC_.query_duration_ms";

    /// <summary>Counter metric name for RiskAssessments command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.ManagementOfChange.RiskAssessmentsLC_.RiskAssessmentsLC_.command_duration_ms";
}