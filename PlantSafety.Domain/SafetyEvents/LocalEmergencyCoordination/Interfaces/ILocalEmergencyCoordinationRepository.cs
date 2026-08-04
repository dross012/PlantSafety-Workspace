// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ILocalEmergencyCoordinationRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LocalEmergencyCoordination aggregate root.
//                  Mirrors PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces.IEmergencyManagementRepository
//                  (tenant-scoped GetByIdAsync, ETag-aware UpdateAsync).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate root.</summary>
public interface ILocalEmergencyCoordinationRepository
{
    /// <summary>Retrieves a LocalEmergencyCoordination aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination?> GetByIdAsync(LocalEmergencyCoordinationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all LocalEmergencyCoordination aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination>> ListAsync(Specifications.LocalEmergencyCoordinationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LocalEmergencyCoordination aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing LocalEmergencyCoordination aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale or mismatched value causes the persistence layer to reject the write as a concurrency conflict.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination aggregate, string? eTag, CancellationToken cancellationToken = default);
}
