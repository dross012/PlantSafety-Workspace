// ============================================================================
// File: LockoutTagoutProcedureDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the LockoutTagoutProcedure module.
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

namespace PlantSafety.Api.Compliance.Procedures.LockoutTagoutProcedure.Diagnostics;

/// <summary>Diagnostic constants for the LockoutTagoutProcedure module.</summary>
public static class LockoutTagoutProcedureDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for LockoutTagoutProcedure traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Procedures.LockoutTagoutProcedure";

    /// <summary>Histogram metric name for LockoutTagoutProcedure query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Procedures.LockoutTagoutProcedureLC_.LockoutTagoutProcedureLC_.query_duration_ms";

    /// <summary>Counter metric name for LockoutTagoutProcedure command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Procedures.LockoutTagoutProcedureLC_.LockoutTagoutProcedureLC_.command_duration_ms";
}