// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           IWorkOrderPriorityRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkOrderPriority aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;

/// <summary>Domain repository interface for the <see cref="WorkOrderPriority"/> aggregate root.</summary>
public interface IWorkOrderPriorityRepository
{
    /// <summary>Retrieves a WorkOrderPriority aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority?> GetByIdAsync(WorkOrderPriorityId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all WorkOrderPriority aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority>> ListAsync(WorkOrderPrioritySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkOrderPriority aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WorkOrderPriority aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, CancellationToken cancellationToken = default);
}