// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           IAITrainingJobRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the AITrainingJob aggregate root.
//                  Distinct from the pre-existing, same-simple-name
//                  PlantSafety.Application.Intelligence.AITraining.Repositories.IAITrainingRepository
//                  (the read-model repository backing the date-range GetAITrainingQuery) -- same
//                  simple name in a different namespace, mirroring
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces.IRCARepository's
//                  identical CQRS read/write repository separation precedent. DI registrations for
//                  both interfaces MUST fully-qualify the type name to avoid an ambiguous-reference
//                  compile error.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.AITraining.Interfaces;

/// <summary>Domain repository interface for the AITrainingJob aggregate root.</summary>
public interface IAITrainingJobRepository
{
    /// <summary>
    /// Retrieves an AITrainingJob aggregate by its identity, scoped to <paramref name="tenantId"/>,
    /// or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob?> GetByIdAsync(
        AITrainingJobId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AITrainingJob aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AITrainingJob aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob aggregate, string? eTag, CancellationToken cancellationToken = default);
}
