// ============================================================================
// File: InternalDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Internal module.
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
namespace PlantSafety.Api.Integrations.Notifications.Internal.Diagnostics;

/// <summary>Diagnostic constants for the Internal module.</summary>
public static class InternalDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Internal traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.Notifications.Internal";

    /// <summary>Histogram metric name for Internal query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Integrations.Notifications.InternalLC_.InternalLC_.query_duration_ms";

    /// <summary>Counter metric name for Internal command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Integrations.Notifications.InternalLC_.InternalLC_.command_duration_ms";
}