// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           ScheduleResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for SchedulePreventiveMaintenanceTaskCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Result of a successful <see cref="SchedulePreventiveMaintenanceTaskCommand"/> execution.</summary>
/// <param name="PreventiveMaintenanceTaskId">Identity of the newly scheduled PreventiveMaintenanceTask aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record SchedulePreventiveMaintenanceTaskResult(
    Guid PreventiveMaintenanceTaskId,
    string Status);
