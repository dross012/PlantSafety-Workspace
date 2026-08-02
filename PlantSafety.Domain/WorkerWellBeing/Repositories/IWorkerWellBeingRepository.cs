// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           IWorkerWellBeingRepository.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: added tenant-scoping to GetByIdAsync and
//                  optimistic-concurrency ETag support to UpdateAsync.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkerWellBeing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Repositories;

/// <summary>Domain repository interface for the <see cref="WorkerWellBeing"/> aggregate root.</summary>
public interface IWorkerWellBeingRepository
{
    /// <summary>Retrieves a WorkerWellBeing aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing?> GetByIdAsync(WorkerWellBeingId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkerWellBeing aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WorkerWellBeing aggregate, enforcing optimistic concurrency via <paramref name="eTag"/>.</summary>
    Task UpdateAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, string? eTag, CancellationToken cancellationToken = default);
}
