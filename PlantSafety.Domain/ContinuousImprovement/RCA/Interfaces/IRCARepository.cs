// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           IRCARepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the RCA aggregate root. Distinct
//                  from the pre-existing, same-simple-name
//                  PlantSafety.Application.ContinuousImprovement.RCA.Repositories.IRCARepository
//                  (the read-model repository backing the date-range GetRCAQuery) -- same simple
//                  name in a different namespace, mirroring
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces
//                  .IRootCauseAnalysisRepository's identical CQRS read/write repository separation
//                  precedent. DI registrations for both interfaces MUST fully-qualify the type
//                  name to avoid an ambiguous-reference compile error (see
//                  ContinuousImprovementRCAAggregateInfrastructureServiceRegistration.cs).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;

/// <summary>Domain repository interface for the RCA aggregate root.</summary>
public interface IRCARepository
{
    /// <summary>
    /// Retrieves an RCA aggregate by its identity, scoped to <paramref name="tenantId"/>, or
    /// <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA?> GetByIdAsync(
        RCAId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RCA aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RCA aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA aggregate, string? eTag, CancellationToken cancellationToken = default);
}
