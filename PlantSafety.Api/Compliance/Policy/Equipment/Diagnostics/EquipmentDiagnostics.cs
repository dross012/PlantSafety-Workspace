// ============================================================================
// File: EquipmentDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the Equipment module.
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

namespace PlantSafety.Api.Compliance.Policy.Equipment.Diagnostics;

/// <summary>Diagnostic constants for the Equipment module.</summary>
public static class EquipmentDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Equipment traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Compliance.Policy.Equipment";

    /// <summary>Histogram metric name for Equipment query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Compliance.Policy.EquipmentLC_.EquipmentLC_.query_duration_ms";

    /// <summary>Counter metric name for Equipment command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Compliance.Policy.EquipmentLC_.EquipmentLC_.command_duration_ms";
}