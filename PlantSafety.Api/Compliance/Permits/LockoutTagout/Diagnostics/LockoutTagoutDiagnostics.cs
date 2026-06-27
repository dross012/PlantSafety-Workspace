// ============================================================================
// File: LockoutTagoutDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the LockoutTagout module.
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

namespace PlantSafety.Api.Compliance.Permits.LockoutTagout.Diagnostics;

/// <summary>Diagnostic constants for the LockoutTagout module.</summary>
public static class LockoutTagoutDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for LockoutTagout traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.LockoutTagout";

    /// <summary>Histogram metric name for LockoutTagout query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.LockoutTagoutLC_.LockoutTagoutLC_.query_duration_ms";

    /// <summary>Counter metric name for LockoutTagout command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.LockoutTagoutLC_.LockoutTagoutLC_.command_duration_ms";
}