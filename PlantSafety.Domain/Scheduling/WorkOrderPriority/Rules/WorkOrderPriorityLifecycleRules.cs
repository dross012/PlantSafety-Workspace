// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the WorkOrderPriority aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="WorkOrderPriority"/> aggregate.</summary>
public static class WorkOrderPriorityLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(WorkOrderPriorityStatus current) => current == WorkOrderPriorityStatus.Open;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(WorkOrderPriorityStatus current) => current == WorkOrderPriorityStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(WorkOrderPriorityStatus current) => current == WorkOrderPriorityStatus.Resolved;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(WorkOrderPriorityStatus current) => current == WorkOrderPriorityStatus.Closed;
}