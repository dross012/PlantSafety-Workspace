// ============================================================================
// File: BehaviorDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Behavior module.
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

namespace PlantSafety.Api.Analytics.Culture.Behavior.Diagnostics;

/// <summary>Diagnostic constants for the Behavior module.</summary>
public static class BehaviorDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Behavior traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Culture.Behavior";

    /// <summary>Histogram metric name for Behavior query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Culture.BehaviorLC_.BehaviorLC_.query_duration_ms";

    /// <summary>Counter metric name for Behavior command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Culture.BehaviorLC_.BehaviorLC_.command_duration_ms";
}