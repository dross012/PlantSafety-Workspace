// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           IRegulatoryCalendarRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the RegulatoryCalendar read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces.
//                  IRegulatoryCalendarRepository (the write-side aggregate repository) -- same
//                  simple name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories;

/// <summary>Read-only repository for RegulatoryCalendar data. Implemented in the Infrastructure layer.</summary>
public interface IRegulatoryCalendarRepository
{
    /// <summary>
    /// Returns the RegulatoryCalendar read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<RegulatoryCalendarItemDto?> GetByIdAsync(Guid regulatoryCalendarId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns RegulatoryCalendar items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RegulatoryCalendarItemDto>> GetRegulatoryCalendarAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
