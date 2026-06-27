// ============================================================================
// File: PPEDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PPE module.
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

namespace PlantSafety.Api.Compliance.Policy.PPE.Diagnostics;

/// <summary>Diagnostic constants for the PPE module.</summary>
public static class PPEDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PPE traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.PPE";

    /// <summary>Histogram metric name for PPE query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.PPELC_.PPELC_.query_duration_ms";

    /// <summary>Counter metric name for PPE command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.PPELC_.PPELC_.command_duration_ms";
}