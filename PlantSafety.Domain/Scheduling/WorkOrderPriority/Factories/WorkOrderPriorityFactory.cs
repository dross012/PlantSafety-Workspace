// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing WorkOrderPriority aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Factories;

/// <summary>Default factory implementation for constructing <see cref="WorkOrderPriority"/> aggregates.</summary>
public sealed class WorkOrderPriorityFactory : IWorkOrderPriorityFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}