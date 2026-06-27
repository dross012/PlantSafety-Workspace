// ============================================================================
// File: ISODiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the ISO module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Renewals.ISO.Diagnostics;

/// <summary>Diagnostic constants for the ISO module.</summary>
public static class ISODiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for ISO traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.RegulatoryCalendar.Renewals.ISO";

    /// <summary>Histogram metric name for ISO query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Renewals.ISOLC_.ISOLC_.query_duration_ms";

    /// <summary>Counter metric name for ISO command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.RegulatoryCalendar.Renewals.ISOLC_.ISOLC_.command_duration_ms";
}