// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Infrastructure
//  File:           PMSchedulingReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PMScheduling Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.PMScheduling.Models;
using PlantSafety.Application.Scheduling.PMScheduling.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.PMScheduling.Persistence;

/// <summary>Dapper-backed repository for PMScheduling read-model queries.</summary>
public sealed class PMSchedulingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PMSchedulingReadDapperRepository> logger)
    : IPMSchedulingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PMSchedulingItemDto>> GetPMSchedulingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PMScheduling tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PMSchedulingItemDto>(
            "[Scheduling].[usp_PMScheduling_GetRange]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PMSchedulingItemDto?> GetByIdAsync(Guid pmSchedulingId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for PMScheduling {PMSchedulingId} Tenant {TenantId}", pmSchedulingId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PMSchedulingId", pmSchedulingId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PMSchedulingItemDto>(
            "[Scheduling].[usp_PMScheduling_GetById]", parameters, cancellationToken);
    }
}
