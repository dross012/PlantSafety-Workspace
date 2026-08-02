// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           IDrillsAndExercisesRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the DrillsAndExercises aggregate
//                  root. Same simple name as, but a different namespace from, the pre-existing
//                  Application-layer read-only list repository interface
//                  PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories
//                  .IDrillsAndExercisesRepository -- mirrors the identical CQRS read/write
//                  repository separation precedent used by Compliance.Permits and
//                  Operational.PredictiveMaintenance. No ListAsync/Specification method (this
//                  codebase's precedent -- RiskManagement's ListAsync -- shows these go unused).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces;

/// <summary>Domain repository interface for the DrillsAndExercises aggregate root.</summary>
public interface IDrillsAndExercisesRepository
{
    /// <summary>
    /// Retrieves a DrillsAndExercises aggregate by its identity, scoped to <paramref name="tenantId"/>,
    /// or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises?> GetByIdAsync(
        DrillsAndExercisesId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DrillsAndExercises aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DrillsAndExercises aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises aggregate, string? eTag, CancellationToken cancellationToken = default);
}
