// ============================================================================
// File: EngagementScoringDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the EngagementScoring module.
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

namespace PlantSafety.Api.Analytics.Culture.EngagementScoring.Diagnostics;

/// <summary>Diagnostic constants for the EngagementScoring module.</summary>
public static class EngagementScoringDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EngagementScoring traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Culture.EngagementScoring";

    /// <summary>Histogram metric name for EngagementScoring query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Culture.EngagementScoringLC_.EngagementScoringLC_.query_duration_ms";

    /// <summary>Counter metric name for EngagementScoring command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Culture.EngagementScoringLC_.EngagementScoringLC_.command_duration_ms";
}