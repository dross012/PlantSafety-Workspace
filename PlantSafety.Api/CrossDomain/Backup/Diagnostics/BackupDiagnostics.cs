// ============================================================================
// File: BackupDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Backup module.
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

namespace PlantSafety.Api.CrossDomain.Backup.Diagnostics;

/// <summary>Diagnostic constants for the Backup module.</summary>
public static class BackupDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Backup traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Backup";

    /// <summary>Histogram metric name for Backup query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.BackupLC_.BackupLC_.query_duration_ms";

    /// <summary>Counter metric name for Backup command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.BackupLC_.BackupLC_.command_duration_ms";
}