// ============================================================================
// File: SchedulesDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Schedules module.
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

namespace PlantSafety.Api.Analytics.Schedules.Diagnostics;

/// <summary>Diagnostic constants for the Schedules module.</summary>
public static class SchedulesDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Schedules traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Schedules";

    /// <summary>Histogram metric name for Schedules query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.SchedulesLC_.SchedulesLC_.query_duration_ms";

    /// <summary>Counter metric name for Schedules command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.SchedulesLC_.SchedulesLC_.command_duration_ms";
}