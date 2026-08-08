// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.MachineSafety
//  Layer:          Domain
//  File:           MachineSafetyStandardsInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input snapshot records accepted by MachineSafetyStandardsEngine. Callers
//                  project the real MachineGuardsBarriersItemDto, LotoSystemsItemDto,
//                  EmergencyStopDevicesItemDto, and TwoHandControlsItemDto read models (plus any
//                  supplementary inspection/audit/test detail the Application layer sources from
//                  related aggregate records) into these snapshot records. No infrastructure/SQL/
//                  HTTP dependency here -- Domain layer, pure/stateless. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.MachineSafety.Services;

/// <summary>Input to <see cref="MachineSafetyStandardsEngine.EvaluateCompliance"/>: a site's machine-safeguarding devices as of a point in time.</summary>
public sealed record MachineSafetyStandardsInput(
    IReadOnlyList<MachineGuardSnapshot> Guards,
    IReadOnlyList<LotoSystemSnapshot> LotoSystems,
    IReadOnlyList<EmergencyStopSnapshot> EmergencyStops,
    IReadOnlyList<TwoHandControlSnapshot> TwoHandControls,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Point-in-time snapshot of a single machine guard/barrier's ISO 14120/ANSI B11.19
/// compliance-relevant state, projected from the real MachineGuardsBarriersItemDto read model
/// (GuardTitle, GuardType, EquipmentLocation, GuardStatus) plus supplementary
/// inspection/deficiency/integrity-score detail the Application-layer handler is expected to
/// source from the MachineGuardsBarriers aggregate's related inspection records.
/// </summary>
public sealed record MachineGuardSnapshot(
    string GuardTitle,
    string GuardType,
    string EquipmentLocation,
    UtcTimestamp? InspectedAtUtc,
    UtcTimestamp? NextInspectionDueAtUtc,
    int DeficiencyCount,
    decimal GuardIntegrityScore,
    string Status);

/// <summary>
/// Point-in-time snapshot of a single LOTO system's ANSI/ASSP Z244.1 compliance-relevant state,
/// projected from the real LotoSystemsItemDto read model (SystemTitle, EnergySourceType,
/// IsolationPointDescription, SystemStatus) plus supplementary isolation-point-count/
/// zero-energy-verification/audit/compliance-score detail the Application-layer handler is
/// expected to source from the LotoSystems aggregate's related verification/audit records.
/// </summary>
public sealed record LotoSystemSnapshot(
    string SystemTitle,
    string EquipmentTag,
    string EnergySourceTypes,
    int IsolationPointCount,
    UtcTimestamp? VerifiedZeroEnergyAtUtc,
    UtcTimestamp? AuditedAtUtc,
    decimal ComplianceScore,
    string Status);

/// <summary>
/// Point-in-time snapshot of a single emergency-stop device's ISO 13850 compliance-relevant
/// state, projected from the real EmergencyStopDevicesItemDto read model (DeviceTitle, DeviceType,
/// EquipmentLocation, DeviceStatus) plus supplementary test-date/failure-count detail the
/// Application-layer handler is expected to source from the EmergencyStopDevices aggregate's
/// related test records.
/// </summary>
public sealed record EmergencyStopSnapshot(
    string DeviceTitle,
    string DeviceType,
    string EquipmentLocation,
    UtcTimestamp? TestedAtUtc,
    UtcTimestamp? NextTestDueAtUtc,
    int FailureCount,
    string Status);

/// <summary>
/// Point-in-time snapshot of a single two-hand control device's ISO 13851/ANSI B11.19
/// compliance-relevant state, projected from the real TwoHandControlsItemDto read model
/// (ControlTitle, ControlType, EquipmentLocation, ControlStatus) plus supplementary
/// test-date/bypass-attempt-count detail the Application-layer handler is expected to source
/// from the TwoHandControls aggregate's related test records.
/// </summary>
public sealed record TwoHandControlSnapshot(
    string ControlTitle,
    string EquipmentType,
    string EquipmentLocation,
    UtcTimestamp? TestedAtUtc,
    UtcTimestamp? NextTestDueAtUtc,
    int BypassAttemptCount,
    string Status);
