// ============================================================================
// File: ComplianceDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Compliance module.
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

namespace PlantSafety.Api.Analytics.ScoreCards.Compliance.Diagnostics;

/// <summary>Diagnostic constants for the Compliance module.</summary>
public static class ComplianceDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Compliance traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.ScoreCards.Compliance";

    /// <summary>Histogram metric name for Compliance query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.ScoreCards.ComplianceLC_.ComplianceLC_.query_duration_ms";

    /// <summary>Counter metric name for Compliance command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.ScoreCards.ComplianceLC_.ComplianceLC_.command_duration_ms";
}