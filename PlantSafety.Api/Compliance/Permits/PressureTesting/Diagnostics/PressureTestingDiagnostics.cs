// ============================================================================
// File: PressureTestingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PressureTesting module.
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

namespace PlantSafety.Api.Compliance.Permits.PressureTesting.Diagnostics;

/// <summary>Diagnostic constants for the PressureTesting module.</summary>
public static class PressureTestingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PressureTesting traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Permits.PressureTesting";

    /// <summary>Histogram metric name for PressureTesting query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Permits.PressureTestingLC_.PressureTestingLC_.query_duration_ms";

    /// <summary>Counter metric name for PressureTesting command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Permits.PressureTestingLC_.PressureTestingLC_.command_duration_ms";
}