// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           IEmergencyDrillPlanningRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyDrillPlanning aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IAccidentRepository.GetByIdAsync(AccidentId, Guid, CancellationToken) and
//                  IPermitsRepository.GetByIdAsync) and UpdateAsync now takes an optimistic-
//                  concurrency ETag (mirroring IPermitsRepository.UpdateAsync). This aggregate
//                  previously had no write path at all -- only the read-only
//                  GetEmergencyDrillPlanningQuery ever touched this module -- so there is no
//                  pre-existing 2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyDrillPlanning"/> aggregate root.</summary>
public interface IEmergencyDrillPlanningRepository
{
    /// <summary>Retrieves a EmergencyDrillPlanning aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning?> GetByIdAsync(EmergencyDrillPlanningId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyDrillPlanning aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning>> ListAsync(EmergencyDrillPlanningSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyDrillPlanning aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing EmergencyDrillPlanning aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, string? eTag, CancellationToken cancellationToken = default);
}