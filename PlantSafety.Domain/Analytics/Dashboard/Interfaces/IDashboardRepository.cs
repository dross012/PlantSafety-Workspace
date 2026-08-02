// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           IDashboardRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Dashboard aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring
//                  PlantSafety.Domain.Compliance.Permits.Interfaces.IPermitsRepository's identical
//                  Phase 4 retrofit. This aggregate previously had no write path at all -- no
//                  Application-layer command ever called these methods -- so there is no
//                  pre-existing call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Interfaces;

/// <summary>Domain repository interface for the <see cref="Dashboard"/> aggregate root.</summary>
public interface IDashboardRepository
{
    /// <summary>Retrieves a Dashboard aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard?> GetByIdAsync(DashboardId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Dashboard aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard>> ListAsync(DashboardSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Dashboard aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Dashboard aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, string? eTag, CancellationToken cancellationToken = default);
}
