// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           IWorkOrderPriorityFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing WorkOrderPriority aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;

/// <summary>Factory interface for constructing <see cref="WorkOrderPriority"/> aggregates.</summary>
public interface IWorkOrderPriorityFactory
{
    /// <summary>Creates a new WorkOrderPriority aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}