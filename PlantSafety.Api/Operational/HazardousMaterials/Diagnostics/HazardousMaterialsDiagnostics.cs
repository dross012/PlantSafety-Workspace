// ============================================================================
// File: HazardousMaterialsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the HazardousMaterials module.
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

namespace PlantSafety.Api.Operational.HazardousMaterials.Diagnostics;

/// <summary>Diagnostic constants for the HazardousMaterials module.</summary>
public static class HazardousMaterialsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for HazardousMaterials traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.HazardousMaterials";

    /// <summary>Histogram metric name for HazardousMaterials query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.HazardousMaterialsLC_.HazardousMaterialsLC_.query_duration_ms";

    /// <summary>Counter metric name for HazardousMaterials command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.HazardousMaterialsLC_.HazardousMaterialsLC_.command_duration_ms";
}