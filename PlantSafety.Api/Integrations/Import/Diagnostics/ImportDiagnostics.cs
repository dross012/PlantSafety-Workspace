// ============================================================================
// File: ImportDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Integrations.Import module.
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

namespace PlantSafety.Api.Integrations.Import.Diagnostics;

/// <summary>Diagnostic constants for the Integrations.Import module.</summary>
public static class ImportDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Integrations.Import traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.Import";

    /// <summary>Histogram metric name for Integrations.Import query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.integrations.import.query_duration_ms";

    /// <summary>Counter metric name for Integrations.Import command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.integrations.import.command_duration_ms";
}
