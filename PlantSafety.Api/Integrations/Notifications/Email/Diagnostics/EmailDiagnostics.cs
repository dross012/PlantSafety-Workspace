// ============================================================================
// File: EmailDiagnostics.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: ActivitySource names and metric name constants for the Email module.
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
namespace PlantSafety.Api.Integrations.Notifications.Email.Diagnostics;

/// <summary>Diagnostic constants for the Email module.</summary>
public static class EmailDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Email traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Integrations.Notifications.Email";

    /// <summary>Histogram metric name for Email query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Integrations.Notifications.EmailLC_.EmailLC_.query_duration_ms";

    /// <summary>Counter metric name for Email command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Integrations.Notifications.EmailLC_.EmailLC_.command_duration_ms";
}