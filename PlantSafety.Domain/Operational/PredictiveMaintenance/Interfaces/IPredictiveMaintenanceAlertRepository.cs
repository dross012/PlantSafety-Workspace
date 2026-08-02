// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           IPredictiveMaintenanceAlertRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the PredictiveMaintenanceAlert
//                  aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;

/// <summary>Domain repository interface for the PredictiveMaintenanceAlert aggregate root.</summary>
public interface IPredictiveMaintenanceAlertRepository
{
    /// <summary>
    /// Retrieves a PredictiveMaintenanceAlert aggregate by its identity, scoped to
    /// <paramref name="tenantId"/>, or <see langword="null"/> if not found or if it belongs to a
    /// different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates.PredictiveMaintenanceAlert?> GetByIdAsync(
        PredictiveMaintenanceAlertId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PredictiveMaintenanceAlert aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates.PredictiveMaintenanceAlert aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PredictiveMaintenanceAlert aggregate, enforcing optimistic
    /// concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates.PredictiveMaintenanceAlert aggregate, string? eTag, CancellationToken cancellationToken = default);
}
