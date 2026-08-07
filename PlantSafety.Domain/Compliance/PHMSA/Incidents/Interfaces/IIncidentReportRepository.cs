// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IIncidentReportRepository.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface (write-side) for the IncidentReport aggregate root.
//                  Additive-only evolution.
//
//                  NOTE (2026-08-07, Application/Infrastructure/Api build-out pass): AddAsync and
//                  UpdateAsync were extended with explicit tenantId (and, for AddAsync, siteId)
//                  parameters. The IncidentReport aggregate itself intentionally carries no
//                  TenantId/SiteId state (unlike PlantSafety.Domain.Compliance.Permits.Aggregates.
//                  Permits), yet [Compliance].[PHMSA_IncidentReport].TenantId is NOT NULL and
//                  every other tenant-scoped repository in this codebase (see
//                  TenantSessionContextExtensions remarks) receives tenantId as an explicit
//                  parameter on every method, not via an ambient accessor. Without this addition
//                  AddAsync would have no way to populate the required TenantId column. This is a
//                  minimal, additive, backward-compatible fix to a genuine gap in the original
//                  interface -- no existing member was removed or renamed.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;

/// <summary>Domain repository interface (write-side) for the <see cref="Aggregates.IncidentReport"/> aggregate root.</summary>
public interface IIncidentReportRepository
{
    /// <summary>Retrieves an IncidentReport aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.IncidentReport?> GetByIdAsync(IncidentReportId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Lists IncidentReport aggregates for the given tenant, optionally bounded by an incident-date window.</summary>
    Task<IReadOnlyList<Aggregates.IncidentReport>> ListAsync(Guid tenantId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);

    /// <summary>Persists a new IncidentReport aggregate, tagging it with the owning tenant (and optional site).</summary>
    /// <param name="aggregate">The newly-created aggregate to persist.</param>
    /// <param name="tenantId">Tenant identifier the new report belongs to. See remarks above.</param>
    /// <param name="siteId">Optional site scope. Null applies tenant-wide.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task AddAsync(Aggregates.IncidentReport aggregate, Guid tenantId, Guid? siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing IncidentReport aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="tenantId">Tenant identifier the report belongs to, for defense-in-depth WHERE-clause scoping. See remarks above.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Aggregates.IncidentReport aggregate, Guid tenantId, string? eTag, CancellationToken cancellationToken = default);
}
