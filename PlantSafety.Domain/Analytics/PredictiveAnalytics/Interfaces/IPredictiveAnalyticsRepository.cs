// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           IPredictiveAnalyticsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PredictiveAnalytics aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IPermitsRepository.GetByIdAsync(PermitsId, Guid, CancellationToken)) and
//                  UpdateAsync now takes an optimistic-concurrency ETag. This aggregate
//                  previously had no write path at all -- only the read-only
//                  GetPredictiveAnalyticsQuery / IPredictiveAnalyticsRepository (Application-layer,
//                  distinct type in a different namespace) ever touched PredictiveAnalytics data.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;

/// <summary>Domain repository interface for the <see cref="PredictiveAnalytics"/> aggregate root.</summary>
public interface IPredictiveAnalyticsRepository
{
    /// <summary>Retrieves a PredictiveAnalytics aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics?> GetByIdAsync(PredictiveAnalyticsId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all PredictiveAnalytics aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics>> ListAsync(PredictiveAnalyticsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PredictiveAnalytics aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PredictiveAnalytics aggregate, enforcing optimistic
    /// concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics aggregate, string? eTag, CancellationToken cancellationToken = default);
}
