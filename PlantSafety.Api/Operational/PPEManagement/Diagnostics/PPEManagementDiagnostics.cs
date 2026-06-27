// ============================================================================
// File: PPEManagementDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the PPEManagement module.
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

namespace PlantSafety.Api.Operational.PPEManagement.Diagnostics;

/// <summary>Diagnostic constants for the PPEManagement module.</summary>
public static class PPEManagementDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for PPEManagement traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.PPEManagement";

    /// <summary>Histogram metric name for PPEManagement query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.PPEManagementLC_.PPEManagementLC_.query_duration_ms";

    /// <summary>Counter metric name for PPEManagement command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.PPEManagementLC_.PPEManagementLC_.command_duration_ms";
}