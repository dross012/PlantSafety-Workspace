// ============================================================================
// File: PermitsToWorkDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PermitsToWork module.
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

namespace PlantSafety.Api.Operational.PermitsToWork.Diagnostics;

/// <summary>Diagnostic constants for the PermitsToWork module.</summary>
public static class PermitsToWorkDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PermitsToWork traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.PermitsToWork";

    /// <summary>Histogram metric name for PermitsToWork query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.PermitsToWorkLC_.PermitsToWorkLC_.query_duration_ms";

    /// <summary>Counter metric name for PermitsToWork command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.PermitsToWorkLC_.PermitsToWorkLC_.command_duration_ms";
}