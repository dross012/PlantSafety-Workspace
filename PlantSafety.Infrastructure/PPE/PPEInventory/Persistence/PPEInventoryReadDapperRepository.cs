// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Infrastructure
//  File:           PPEInventoryReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPEInventory Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.PPEInventory.Models;
using PlantSafety.Application.PPE.PPEInventory.Repositories;

namespace PlantSafety.Infrastructure.PPE.PPEInventory.Persistence;

/// <summary>Dapper-backed repository for PPEInventory read-model queries.</summary>
public sealed class PPEInventoryReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPEInventoryReadDapperRepository> logger)
    : IPPEInventoryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEInventoryItemDto>> GetPPEInventoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPEInventory tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEInventoryItemDto>(
            "[PPE].[usp_PPEInventory_GetRange]", parameters, cancellationToken);
    }
}
