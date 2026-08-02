// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           IRootCauseAnalysisRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the RootCauseAnalysis aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;

/// <summary>Domain repository interface for the RootCauseAnalysis aggregate root.</summary>
public interface IRootCauseAnalysisRepository
{
    /// <summary>
    /// Retrieves a RootCauseAnalysis aggregate by its identity, scoped to
    /// <paramref name="tenantId"/>, or <see langword="null"/> if not found or if it belongs to a
    /// different tenant.
    /// </summary>
    Task<PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis?> GetByIdAsync(
        RootCauseAnalysisId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RootCauseAnalysis aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing RootCauseAnalysis aggregate, enforcing optimistic
    /// concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis aggregate, string? eTag, CancellationToken cancellationToken = default);
}
