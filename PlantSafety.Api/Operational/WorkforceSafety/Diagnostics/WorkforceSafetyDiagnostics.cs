// ============================================================================
// File: WorkforceSafetyDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: ActivitySource names and metric name constants for the WorkforceSafety module.
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

namespace PlantSafety.Api.Operational.WorkforceSafety.Diagnostics;

/// <summary>Diagnostic constants for the WorkforceSafety module.</summary>
public static class WorkforceSafetyDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for WorkforceSafety traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.WorkforceSafety";

    /// <summary>Histogram metric name for WorkforceSafety query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.WorkforceSafetyLC_.WorkforceSafetyLC_.query_duration_ms";

    /// <summary>Counter metric name for WorkforceSafety command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.WorkforceSafetyLC_.WorkforceSafetyLC_.command_duration_ms";
}