// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           IPreventiveMaintenanceTaskRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the PreventiveMaintenanceTask
//                  aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;

/// <summary>Domain repository interface for the PreventiveMaintenanceTask aggregate root.</summary>
public interface IPreventiveMaintenanceTaskRepository
{
    /// <summary>
    /// Retrieves a PreventiveMaintenanceTask aggregate by its identity, scoped to
    /// <paramref name="tenantId"/>, or <see langword="null"/> if not found or if it belongs to a
    /// different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates.PreventiveMaintenanceTask?> GetByIdAsync(
        PreventiveMaintenanceTaskId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PreventiveMaintenanceTask aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates.PreventiveMaintenanceTask aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PreventiveMaintenanceTask aggregate, enforcing optimistic
    /// concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates.PreventiveMaintenanceTask aggregate, string? eTag, CancellationToken cancellationToken = default);
}
