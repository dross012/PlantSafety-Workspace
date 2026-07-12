// ============================================================================
// File: DataLakeDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the DataLake module.
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
namespace PlantSafety.Api.Intelligence.DataLake.Diagnostics;

/// <summary>Diagnostic constants for the DataLake module.</summary>
public static class DataLakeDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for DataLake traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Intelligence.DataLake";

    /// <summary>Histogram metric name for DataLake query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Intelligence.DataLakeLC_.DataLakeLC_.query_duration_ms";

    /// <summary>Counter metric name for DataLake command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Intelligence.DataLakeLC_.DataLakeLC_.command_duration_ms";
}