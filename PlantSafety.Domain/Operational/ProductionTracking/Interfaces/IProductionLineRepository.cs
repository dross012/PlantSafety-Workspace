// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           IProductionLineRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the ProductionLine aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Interfaces;

/// <summary>Domain repository interface for the ProductionLine aggregate root.</summary>
public interface IProductionLineRepository
{
    /// <summary>
    /// Retrieves a ProductionLine aggregate by its identity, scoped to <paramref name="tenantId"/>,
    /// or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<ProductionLine?> GetByIdAsync(ProductionLineId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ProductionLine aggregate.</summary>
    Task AddAsync(ProductionLine aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ProductionLine aggregate.</summary>
    Task UpdateAsync(ProductionLine aggregate, CancellationToken cancellationToken = default);
}
