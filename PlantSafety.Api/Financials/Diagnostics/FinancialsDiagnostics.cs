// ============================================================================
// File: FinancialsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Financials module.
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

namespace PlantSafety.Api.Financials.Diagnostics;

/// <summary>Diagnostic constants for the Financials module.</summary>
public static class FinancialsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Financials traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Financials";

    /// <summary>Histogram metric name for Financials query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.FinancialsLC_.FinancialsLC_.query_duration_ms";

    /// <summary>Counter metric name for Financials command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.FinancialsLC_.FinancialsLC_.command_duration_ms";
}