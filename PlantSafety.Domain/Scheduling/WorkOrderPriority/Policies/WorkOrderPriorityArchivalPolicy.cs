// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for WorkOrderPriority aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Policies;

/// <summary>Determines whether a <see cref="WorkOrderPriority"/> aggregate is eligible for archival.</summary>
public static class WorkOrderPriorityArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the WorkOrderPriority may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate)
        => aggregate.Status == WorkOrderPriorityStatus.Closed;
}