// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           IAIInvestigationRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the AIInvestigation aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;

/// <summary>Domain repository interface for the AIInvestigation aggregate root.</summary>
public interface IAIInvestigationRepository
{
    /// <summary>
    /// Retrieves an AIInvestigation aggregate by its identity, scoped to
    /// <paramref name="tenantId"/>, or <see langword="null"/> if not found or if it belongs to a
    /// different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation?> GetByIdAsync(
        AIInvestigationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AIInvestigation aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AIInvestigation aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation aggregate, string? eTag, CancellationToken cancellationToken = default);
}
