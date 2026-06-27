// ============================================================================
// File: PlanningProjectsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PlanningProjects module.
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

namespace PlantSafety.Api.PlanningProjects.Diagnostics;

/// <summary>Diagnostic constants for the PlanningProjects module.</summary>
public static class PlanningProjectsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PlanningProjects traces.</summary>
    public const string ActivitySourceName = "PlantSafety.PlanningProjects";

    /// <summary>Histogram metric name for PlanningProjects query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.PlanningProjectsLC_.PlanningProjectsLC_.query_duration_ms";

    /// <summary>Counter metric name for PlanningProjects command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.PlanningProjectsLC_.PlanningProjectsLC_.command_duration_ms";
}