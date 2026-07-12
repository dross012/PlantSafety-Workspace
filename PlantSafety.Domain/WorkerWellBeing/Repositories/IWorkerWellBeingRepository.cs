// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           IWorkerWellBeingRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkerWellBeing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Repositories;

/// <summary>Domain repository interface for the <see cref="WorkerWellBeing"/> aggregate root.</summary>
public interface IWorkerWellBeingRepository
{
    /// <summary>Retrieves a WorkerWellBeing aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing?> GetByIdAsync(WorkerWellBeingId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkerWellBeing aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WorkerWellBeing aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, CancellationToken cancellationToken = default);
}
