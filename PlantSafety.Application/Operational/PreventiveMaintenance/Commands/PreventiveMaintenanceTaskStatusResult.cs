// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           PreventiveMaintenanceTaskStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the PreventiveMaintenanceTask lifecycle-transition
//                  commands (Start, Complete, Verify, Cancel, MarkOverdue). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Result payload shared by PreventiveMaintenanceTask lifecycle-transition commands.</summary>
/// <param name="PreventiveMaintenanceTaskId">Identity of the affected PreventiveMaintenanceTask aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PreventiveMaintenanceTaskStatusResult(Guid PreventiveMaintenanceTaskId, string Status);
