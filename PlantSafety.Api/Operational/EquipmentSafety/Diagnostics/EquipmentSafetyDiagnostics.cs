// ============================================================================
// File: EquipmentSafetyDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: ActivitySource names and metric name constants for the EquipmentSafety module.
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

namespace PlantSafety.Api.Operational.EquipmentSafety.Diagnostics;

/// <summary>Diagnostic constants for the EquipmentSafety module.</summary>
public static class EquipmentSafetyDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for EquipmentSafety traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Operational.EquipmentSafety";

    /// <summary>Histogram metric name for EquipmentSafety query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.Operational.EquipmentSafetyLC_.EquipmentSafetyLC_.query_duration_ms";

    /// <summary>Counter metric name for EquipmentSafety command duration in milliseconds.</summary>
    public const string CommandDurationMetric = "plantsafety.Operational.EquipmentSafetyLC_.EquipmentSafetyLC_.command_duration_ms";
}