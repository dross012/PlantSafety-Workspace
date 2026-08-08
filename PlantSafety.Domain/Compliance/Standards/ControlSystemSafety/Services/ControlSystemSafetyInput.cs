// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.ControlSystemSafety
//  Layer:          Domain
//  File:           ControlSystemSafetyInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input snapshot records accepted by ControlSystemSafetyEngine. This codebase
//                  does not yet track a formal ISO 13849-1 Performance Level/Category rating on
//                  any real aggregate -- SafetyValvesRelaysItemDto and PresenceSensingDevicesItemDto
//                  (PlantSafety.Application.MachineSafety.SafetyValvesRelays /
//                  PresenceSensingDevices) carry only ComponentType/DeviceType, EquipmentLocation,
//                  and status/inspection fields, with "RedundantControlSystem" /
//                  "DualChannelSafetyRelay" as the closest architectural hint toward a
//                  Category-2/3-style redundant design. RequiredPerformanceLevel/
//                  AchievedPerformanceLevel/Category are therefore caller-supplied per component
//                  (sourced from a documented risk assessment), while InspectedAtUtc/
//                  NextInspectionDueAtUtc/FaultCount/ReliabilityOrAccuracyScore are intended to be
//                  populated from the real SafetyValvesRelays/PresenceSensingDevices data as
//                  supporting evidence. This gap is documented honestly rather than fabricating a
//                  PL/Category field on the real DTOs. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Services;

/// <summary>Input to <see cref="ControlSystemSafetyEngine.EvaluateCompliance"/>: a site's safety-related control system components as of a point in time.</summary>
public sealed record ControlSystemSafetyInput(
    IReadOnlyList<SafetyComponentSnapshot> Components,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Point-in-time snapshot of a single safety-related control system component's ISO 13849-1/
/// IEC 62061 compliance-relevant state. RequiredPerformanceLevel/AchievedPerformanceLevel/
/// Category are caller-supplied (this codebase has no formal PL/Category aggregate field yet);
/// InspectedAtUtc/NextInspectionDueAtUtc/FaultCount/ReliabilityOrAccuracyScore are intended to be
/// sourced from the real SafetyValvesRelaysItemDto/PresenceSensingDevicesItemDto read models.
/// </summary>
public sealed record SafetyComponentSnapshot(
    string ComponentTitle,
    string ComponentType,
    string EquipmentLocation,
    PerformanceLevel RequiredPerformanceLevel,
    PerformanceLevel? AchievedPerformanceLevel,
    ControlSystemCategory? Category,
    UtcTimestamp? InspectedAtUtc,
    UtcTimestamp? NextInspectionDueAtUtc,
    int FaultCount,
    decimal? ReliabilityOrAccuracyScore);
