// ============================================================================
// File: AdministrationDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Administration module.
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

namespace PlantSafety.Api.CrossDomain.Administration.Diagnostics;

/// <summary>Diagnostic constants for the Administration module.</summary>
public static class AdministrationDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Administration traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Administration";

    /// <summary>Histogram metric name for Administration query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.AdministrationLC_.AdministrationLC_.query_duration_ms";

    /// <summary>Counter metric name for Administration command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.AdministrationLC_.AdministrationLC_.command_duration_ms";
}