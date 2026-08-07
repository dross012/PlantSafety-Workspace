// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           IIncidentReportRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the IncidentReport read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces.IIncidentReportRepository
//                  (the write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.
//                  IPermitsRepository's identical precedent for CQRS read/write repository
//                  separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Incidents.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Repositories;

/// <summary>Read-only repository for IncidentReport data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentReportRepository
{
    /// <summary>
    /// Returns the IncidentReport read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<IncidentReportDto?> GetByIdAsync(Guid incidentReportId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns IncidentReport items for the tenant, optionally filtered by site, an inclusive UTC
    /// incident-date range, and PHMSA reportability.
    /// </summary>
    Task<IReadOnlyList<IncidentReportDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime? fromUtc,
        DateTime? toUtc,
        bool? isReportableToPhmsa,
        CancellationToken cancellationToken);
}
