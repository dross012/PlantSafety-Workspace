// ============================================================================
// File: CAPADiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the CAPA module.
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

namespace PlantSafety.Api.ContinuousImprovement.CAPA.Diagnostics;

/// <summary>Diagnostic constants for the CAPA module.</summary>
public static class CAPADiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for CAPA traces.</summary>
    public const string ActivitySourceName = "PlantSafety.ContinuousImprovement.CAPA";

    /// <summary>Histogram metric name for CAPA query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.ContinuousImprovement.CAPALC_.CAPALC_.query_duration_ms";

    /// <summary>Counter metric name for CAPA command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.ContinuousImprovement.CAPALC_.CAPALC_.command_duration_ms";
}