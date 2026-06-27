// ============================================================================
// File: ApprovalsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Approvals module.
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

namespace PlantSafety.Api.Compliance.Policy.Approvals.Diagnostics;

/// <summary>Diagnostic constants for the Approvals module.</summary>
public static class ApprovalsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Approvals traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.Approvals";

    /// <summary>Histogram metric name for Approvals query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.ApprovalsLC_.ApprovalsLC_.query_duration_ms";

    /// <summary>Counter metric name for Approvals command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.ApprovalsLC_.ApprovalsLC_.command_duration_ms";
}