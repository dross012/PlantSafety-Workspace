// ============================================================================
// File: MOCWorkflowsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the MOCWorkflows module.
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

namespace PlantSafety.Api.Compliance.ManagementOfChange.MOCWorkflows.Diagnostics;

/// <summary>Diagnostic constants for the MOCWorkflows module.</summary>
public static class MOCWorkflowsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for MOCWorkflows traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.ManagementOfChange.MOCWorkflows";

    /// <summary>Histogram metric name for MOCWorkflows query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.ManagementOfChange.MOCWorkflowsLC_.MOCWorkflowsLC_.query_duration_ms";

    /// <summary>Counter metric name for MOCWorkflows command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.ManagementOfChange.MOCWorkflowsLC_.MOCWorkflowsLC_.command_duration_ms";
}