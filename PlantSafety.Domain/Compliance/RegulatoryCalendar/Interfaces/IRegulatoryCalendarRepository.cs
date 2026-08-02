// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           IRegulatoryCalendarRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RegulatoryCalendar aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IPermitsRepository.GetByIdAsync(PermitsId, Guid, CancellationToken)) and
//                  UpdateAsync now takes an optimistic-concurrency ETag (mirroring
//                  IPermitsRepository.UpdateAsync(Permits, string?, CancellationToken)). This
//                  aggregate previously had no write path at all -- no Application-layer command
//                  ever called these methods -- so there is no pre-existing call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;

/// <summary>Domain repository interface for the <see cref="RegulatoryCalendar"/> aggregate root.</summary>
public interface IRegulatoryCalendarRepository
{
    /// <summary>Retrieves a RegulatoryCalendar aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar?> GetByIdAsync(RegulatoryCalendarId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all RegulatoryCalendar aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar>> ListAsync(RegulatoryCalendarSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RegulatoryCalendar aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing RegulatoryCalendar aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, string? eTag, CancellationToken cancellationToken = default);
}
