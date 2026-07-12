// ============================================================================
// File: AllInvestigationsDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the AllInvestigations module.
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
namespace PlantSafety.Api.Intelligence.AllInvestigations.Diagnostics;

/// <summary>Diagnostic constants for the AllInvestigations module.</summary>
public static class AllInvestigationsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for AllInvestigations traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Intelligence.AllInvestigations";

    /// <summary>Histogram metric name for AllInvestigations query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Intelligence.AllInvestigationsLC_.AllInvestigationsLC_.query_duration_ms";

    /// <summary>Counter metric name for AllInvestigations command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Intelligence.AllInvestigationsLC_.AllInvestigationsLC_.command_duration_ms";
}