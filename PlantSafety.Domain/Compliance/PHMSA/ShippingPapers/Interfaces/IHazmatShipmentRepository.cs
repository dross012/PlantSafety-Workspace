// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           IHazmatShipmentRepository.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazmatShipment aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazmatShipment"/> aggregate root.</summary>
public interface IHazmatShipmentRepository
{
    /// <summary>Retrieves a HazmatShipment aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<HazmatShipment?> GetByIdAsync(HazmatShipmentId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazmatShipment aggregate.</summary>
    Task AddAsync(HazmatShipment aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing HazmatShipment aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(HazmatShipment aggregate, string? eTag, CancellationToken cancellationToken = default);
}
