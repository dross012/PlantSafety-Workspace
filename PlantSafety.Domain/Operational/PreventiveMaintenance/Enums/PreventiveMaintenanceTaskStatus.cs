// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTaskStatus.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PreventiveMaintenanceTask aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Enums;

/// <summary>Lifecycle states for the PreventiveMaintenanceTask aggregate root.</summary>
public enum PreventiveMaintenanceTaskStatus
{
    /// <summary>Task has been scheduled and awaits work to begin.</summary>
    Scheduled = 0,

    /// <summary>Maintenance work is in progress.</summary>
    InProgress = 1,

    /// <summary>Maintenance work has been completed.</summary>
    Completed = 2,

    /// <summary>Completed work has been independently verified.</summary>
    Verified = 3,

    /// <summary>The task was cancelled before completion.</summary>
    Cancelled = 4,

    /// <summary>The task passed its scheduled date without starting/completing.</summary>
    Overdue = 5,
}
