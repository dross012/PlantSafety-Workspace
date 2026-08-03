// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           IPartRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the Part aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Interfaces;

/// <summary>Domain repository interface for the Part aggregate root.</summary>
public interface IPartRepository
{
    /// <summary>
    /// Retrieves a Part aggregate by its identity, scoped to <paramref name="tenantId"/>, or
    /// <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<Part?> GetByIdAsync(PartId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Part aggregate.</summary>
    Task AddAsync(Part aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Part aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale or mismatched value causes the persistence layer to reject the write.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Part aggregate, string? eTag, CancellationToken cancellationToken = default);
}
