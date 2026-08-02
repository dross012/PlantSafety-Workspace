// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           ScheduleEmergencyDrillPlanningResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for ScheduleEmergencyDrillPlanningCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Result of a successful <see cref="ScheduleEmergencyDrillPlanningCommand"/> execution.</summary>
/// <param name="EmergencyDrillPlanningId">Identity of the newly created EmergencyDrillPlanning aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record ScheduleEmergencyDrillPlanningResult(
    Guid EmergencyDrillPlanningId,
    string Status);
