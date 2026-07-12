// ============================================================================
// File: IoTDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Integrations.IoT module.
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

namespace PlantSafety.Api.Integrations.IoT.Diagnostics;

/// <summary>Diagnostic constants for the Integrations.IoT module.</summary>
public static class IoTDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Integrations.IoT traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.IoT";

    /// <summary>Histogram metric name for Integrations.IoT query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.integrations.iot.query_duration_ms";

    /// <summary>Counter metric name for Integrations.IoT command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.integrations.iot.command_duration_ms";
}
