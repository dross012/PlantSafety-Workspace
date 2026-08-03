// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           IProductionRunRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the ProductionRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Interfaces;

/// <summary>Domain repository interface for the ProductionRun aggregate root.</summary>
public interface IProductionRunRepository
{
    /// <summary>
    /// Retrieves a ProductionRun aggregate by its identity, scoped to <paramref name="tenantId"/>,
    /// or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<ProductionRun?> GetByIdAsync(ProductionRunId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ProductionRun aggregate.</summary>
    Task AddAsync(ProductionRun aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ProductionRun aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale or mismatched value causes the persistence layer to reject the write.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(ProductionRun aggregate, string? eTag, CancellationToken cancellationToken = default);
}
