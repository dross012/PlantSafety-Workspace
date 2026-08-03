// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderStatus.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the WorkOrder aggregate root. New-build module backing
//                  the Maintenance UI's work-order core (distinct from, and deliberately
//                  namespaced away from, Operational.PreventiveMaintenance and
//                  Operational.PredictiveMaintenance -- see docs/Phase5/ui-gaps-scoping.md
//                  "Maintenance" section). Plain simple enum, mirroring the sibling
//                  PreventiveMaintenanceTaskStatus precedent (pure lifecycle vocabulary, no
//                  metadata/behavior attached to the values themselves -- Handbook Rule 2.7B).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Enums;

/// <summary>Lifecycle states for the WorkOrder aggregate root.</summary>
public enum WorkOrderStatus
{
    /// <summary>Work order has been created and awaits technician assignment.</summary>
    Open = 0,

    /// <summary>A technician has been assigned; work has not yet started.</summary>
    Assigned = 1,

    /// <summary>Maintenance work is actively underway.</summary>
    InProgress = 2,

    /// <summary>Work has been paused (e.g. awaiting parts) without being cancelled.</summary>
    OnHold = 3,

    /// <summary>Maintenance work has been completed.</summary>
    Completed = 4,

    /// <summary>The work order was cancelled before completion.</summary>
    Cancelled = 5,
}
