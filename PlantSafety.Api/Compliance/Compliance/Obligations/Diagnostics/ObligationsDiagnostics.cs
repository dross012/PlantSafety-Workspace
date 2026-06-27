// ============================================================================
// File: ObligationsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Obligations module.
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

namespace PlantSafety.Api.Compliance.Compliance.Obligations.Diagnostics;

/// <summary>Diagnostic constants for the Obligations module.</summary>
public static class ObligationsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Obligations traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Compliance.Obligations";

    /// <summary>Histogram metric name for Obligations query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Compliance.ObligationsLC_.ObligationsLC_.query_duration_ms";

    /// <summary>Counter metric name for Obligations command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Compliance.ObligationsLC_.ObligationsLC_.command_duration_ms";
}