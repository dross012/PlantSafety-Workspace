// ============================================================================
// File: IncidentManagementDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the IncidentManagement module.
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

namespace PlantSafety.Api.SafetyEvents.IncidentManagement.Diagnostics;

/// <summary>Diagnostic constants for the IncidentManagement module.</summary>
public static class IncidentManagementDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for IncidentManagement traces.</summary>
    public const string ActivitySourceName = "PlantSafety.SafetyEvents.IncidentManagement";

    /// <summary>Histogram metric name for IncidentManagement query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.SafetyEvents.IncidentManagementLC_.IncidentManagementLC_.query_duration_ms";

    /// <summary>Counter metric name for IncidentManagement command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.SafetyEvents.IncidentManagementLC_.IncidentManagementLC_.command_duration_ms";
}