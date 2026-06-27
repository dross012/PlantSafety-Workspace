// ============================================================================
// File: WorkInstructionsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the WorkInstructions module.
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

namespace PlantSafety.Api.Compliance.Procedures.WorkInstructions.Diagnostics;

/// <summary>Diagnostic constants for the WorkInstructions module.</summary>
public static class WorkInstructionsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for WorkInstructions traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.WorkInstructions";

    /// <summary>Histogram metric name for WorkInstructions query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.WorkInstructionsLC_.WorkInstructionsLC_.query_duration_ms";

    /// <summary>Counter metric name for WorkInstructions command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.WorkInstructionsLC_.WorkInstructionsLC_.command_duration_ms";
}