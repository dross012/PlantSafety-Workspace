// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Infrastructure
//  File:           PPEZoneMappingReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPEZoneMapping Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.PPEZoneMapping.Models;
using PlantSafety.Application.PPE.PPEZoneMapping.Repositories;

namespace PlantSafety.Infrastructure.PPE.PPEZoneMapping.Persistence;

/// <summary>Dapper-backed repository for PPEZoneMapping read-model queries.</summary>
public sealed class PPEZoneMappingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPEZoneMappingReadDapperRepository> logger)
    : IPPEZoneMappingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEZoneMappingItemDto>> GetPPEZoneMappingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPEZoneMapping tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEZoneMappingItemDto>(
            "[PPE].[usp_PPEZoneMapping_GetRange]", parameters, cancellationToken);
    }
}
