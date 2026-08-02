// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Engagement Application-layer read-model. Backs
//                  GetEngagementByIdQuery and GetEngagementRecordsQuery. Reuses
//                  usp_EngagementRecords_GetById (the same proc EngagementAggregateDapperRepository.
//                  GetByIdAsync uses to hydrate the write-side aggregate) and
//                  usp_EngagementRecords_GetRange rather than introducing duplicate stored
//                  procedures. Mirrors PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Engagement.Models;
using PlantSafety.Application.Analytics.Engagement.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Engagement.Persistence;

/// <summary>Dapper-backed repository for Engagement read-model queries.</summary>
public sealed class EngagementReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EngagementReadDapperRepository> logger)
    : IEngagementRepository
{
    /// <inheritdoc/>
    public async Task<EngagementItemDto?> GetByIdAsync(Guid engagementId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Engagement {EngagementId} Tenant {TenantId}", engagementId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EngagementId", engagementId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EngagementItemDto>(
            "[Analytics].[usp_EngagementRecords_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EngagementItemDto>> GetEngagementRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for Engagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EngagementItemDto>(
            "[Analytics].[usp_EngagementRecords_GetRange]", parameters, cancellationToken);
    }
}
