// ============================================================================
// File: UsersDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Users module.
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

namespace PlantSafety.Api.CrossDomain.Users.Diagnostics;

/// <summary>Diagnostic constants for the Users module.</summary>
public static class UsersDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Users traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Users";

    /// <summary>Histogram metric name for Users query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.UsersLC_.UsersLC_.query_duration_ms";

    /// <summary>Counter metric name for Users command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.UsersLC_.UsersLC_.command_duration_ms";
}