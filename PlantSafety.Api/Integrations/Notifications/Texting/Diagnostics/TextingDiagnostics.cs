// ============================================================================
// File: TextingDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Texting module.
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
namespace PlantSafety.Api.Integrations.Notifications.Texting.Diagnostics;

/// <summary>Diagnostic constants for the Texting module.</summary>
public static class TextingDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Texting traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.Notifications.Texting";

    /// <summary>Histogram metric name for Texting query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Integrations.Notifications.TextingLC_.TextingLC_.query_duration_ms";

    /// <summary>Counter metric name for Texting command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Integrations.Notifications.TextingLC_.TextingLC_.command_duration_ms";
}