// ============================================================================
// File: RiskManagementDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the RiskManagement module.
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

namespace PlantSafety.Api.Operational.RiskManagement.Diagnostics;

/// <summary>Diagnostic constants for the RiskManagement module.</summary>
public static class RiskManagementDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for RiskManagement traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.RiskManagement";

    /// <summary>Histogram metric name for RiskManagement query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.RiskManagementLC_.RiskManagementLC_.query_duration_ms";

    /// <summary>Counter metric name for RiskManagement command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.RiskManagementLC_.RiskManagementLC_.command_duration_ms";
}