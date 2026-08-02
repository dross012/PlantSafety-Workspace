// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           IEngagementRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Engagement read-model. Implemented in
//                  the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Analytics.Engagement.Interfaces.IEngagementRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Engagement.Models;

namespace PlantSafety.Application.Analytics.Engagement.Repositories;

/// <summary>Read-only repository for Engagement data. Implemented in the Infrastructure layer.</summary>
public interface IEngagementRepository
{
    /// <summary>
    /// Returns the Engagement read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<EngagementItemDto?> GetByIdAsync(Guid engagementId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns Engagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EngagementItemDto>> GetEngagementRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
