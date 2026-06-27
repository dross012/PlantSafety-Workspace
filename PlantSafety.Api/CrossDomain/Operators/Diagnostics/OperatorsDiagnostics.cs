// ============================================================================
// File: OperatorsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Operators module.
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

namespace PlantSafety.Api.CrossDomain.Operators.Diagnostics;

/// <summary>Diagnostic constants for the Operators module.</summary>
public static class OperatorsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Operators traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Operators";

    /// <summary>Histogram metric name for Operators query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.OperatorsLC_.OperatorsLC_.query_duration_ms";

    /// <summary>Counter metric name for Operators command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.OperatorsLC_.OperatorsLC_.command_duration_ms";
}