// ============================================================================
// File: MachinesDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Machines module.
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

namespace PlantSafety.Api.CrossDomain.Machines.Diagnostics;

/// <summary>Diagnostic constants for the Machines module.</summary>
public static class MachinesDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Machines traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Machines";

    /// <summary>Histogram metric name for Machines query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.MachinesLC_.MachinesLC_.query_duration_ms";

    /// <summary>Counter metric name for Machines command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.MachinesLC_.MachinesLC_.command_duration_ms";
}