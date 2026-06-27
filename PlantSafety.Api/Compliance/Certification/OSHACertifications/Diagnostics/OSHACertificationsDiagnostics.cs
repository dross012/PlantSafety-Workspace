// ============================================================================
// File: OSHACertificationsDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the OSHACertifications module.
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

namespace PlantSafety.Api.Compliance.Certification.OSHACertifications.Diagnostics;

/// <summary>Diagnostic constants for the OSHACertifications module.</summary>
public static class OSHACertificationsDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for OSHACertifications traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Certification.OSHACertifications";

    /// <summary>Histogram metric name for OSHACertifications query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Certification.OSHACertificationsLC_.OSHACertificationsLC_.query_duration_ms";

    /// <summary>Counter metric name for OSHACertifications command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Certification.OSHACertificationsLC_.OSHACertificationsLC_.command_duration_ms";
}