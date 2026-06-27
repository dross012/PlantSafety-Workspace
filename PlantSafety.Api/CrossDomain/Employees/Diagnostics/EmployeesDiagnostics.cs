// ============================================================================
// File: EmployeesDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Employees module.
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

namespace PlantSafety.Api.CrossDomain.Employees.Diagnostics;

/// <summary>Diagnostic constants for the Employees module.</summary>
public static class EmployeesDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Employees traces.</summary>
    public const string ActivitySourceName = "PlantSafety.CrossDomain.Employees";

    /// <summary>Histogram metric name for Employees query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.CrossDomain.EmployeesLC_.EmployeesLC_.query_duration_ms";

    /// <summary>Counter metric name for Employees command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.CrossDomain.EmployeesLC_.EmployeesLC_.command_duration_ms";
}