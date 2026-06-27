// ============================================================================
// File: KPISnapshotsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the KPISnapshots module.
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

namespace PlantSafety.Api.Analytics.Dashboard.KPISnapshots.Diagnostics;

/// <summary>Diagnostic constants for the KPISnapshots module.</summary>
public static class KPISnapshotsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for KPISnapshots traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Dashboard.KPISnapshots";

    /// <summary>Histogram metric name for KPISnapshots query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Analytics.Dashboard.KPISnapshotsLC_.KPISnapshotsLC_.query_duration_ms";

    /// <summary>Counter metric name for KPISnapshots command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Analytics.Dashboard.KPISnapshotsLC_.KPISnapshotsLC_.command_duration_ms";
}