// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderLifecycleRules.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Guards for the WorkOrder aggregate's lifecycle transitions. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Enums;

namespace PlantSafety.Domain.Operational.WorkOrders.Rules;

/// <summary>Defines permitted lifecycle transitions for the WorkOrder aggregate.</summary>
public static class WorkOrderLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a technician may be (re)assigned from <paramref name="current"/>.</summary>
    public static bool CanAssign(WorkOrderStatus current) =>
        current == WorkOrderStatus.Open || current == WorkOrderStatus.Assigned;

    /// <summary>Returns <see langword="true"/> when work may start from <paramref name="current"/>.</summary>
    public static bool CanStart(WorkOrderStatus current) => current == WorkOrderStatus.Assigned;

    /// <summary>Returns <see langword="true"/> when the work order may be put on hold from <paramref name="current"/>.</summary>
    public static bool CanPutOnHold(WorkOrderStatus current) => current == WorkOrderStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when a held work order may resume from <paramref name="current"/>.</summary>
    public static bool CanResume(WorkOrderStatus current) => current == WorkOrderStatus.OnHold;

    /// <summary>Returns <see langword="true"/> when the work order may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(WorkOrderStatus current) => current == WorkOrderStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the work order may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(WorkOrderStatus current) =>
        current is WorkOrderStatus.Open or WorkOrderStatus.Assigned or WorkOrderStatus.InProgress or WorkOrderStatus.OnHold;
}
