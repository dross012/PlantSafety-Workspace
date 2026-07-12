// ============================================================================
// File: TextingDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Integrations.Texting module.
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

namespace PlantSafety.Api.Integrations.Texting.Diagnostics;

/// <summary>Diagnostic constants for the Integrations.Texting module.</summary>
public static class TextingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Integrations.Texting traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.Texting";

    /// <summary>Histogram metric name for Integrations.Texting query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.integrations.texting.query_duration_ms";

    /// <summary>Counter metric name for Integrations.Texting command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.integrations.texting.command_duration_ms";
}
