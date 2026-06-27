// ============================================================================
// File: RegulatoryRequirementsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the RegulatoryRequirements module.
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

namespace PlantSafety.Api.Compliance.Compliance.RegulatoryRequirements.Diagnostics;

/// <summary>Diagnostic constants for the RegulatoryRequirements module.</summary>
public static class RegulatoryRequirementsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for RegulatoryRequirements traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Compliance.RegulatoryRequirements";

    /// <summary>Histogram metric name for RegulatoryRequirements query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Compliance.RegulatoryRequirementsLC_.RegulatoryRequirementsLC_.query_duration_ms";

    /// <summary>Counter metric name for RegulatoryRequirements command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Compliance.RegulatoryRequirementsLC_.RegulatoryRequirementsLC_.command_duration_ms";
}