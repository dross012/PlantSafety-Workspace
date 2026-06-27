// ============================================================================
// File: RevisionsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Revisions module.
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

namespace PlantSafety.Api.Compliance.Policy.Revisions.Diagnostics;

/// <summary>Diagnostic constants for the Revisions module.</summary>
public static class RevisionsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Revisions traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.Revisions";

    /// <summary>Histogram metric name for Revisions query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.RevisionsLC_.RevisionsLC_.query_duration_ms";

    /// <summary>Counter metric name for Revisions command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.RevisionsLC_.RevisionsLC_.command_duration_ms";
}