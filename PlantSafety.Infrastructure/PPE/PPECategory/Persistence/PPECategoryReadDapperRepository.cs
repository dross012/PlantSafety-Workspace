// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Infrastructure
//  File:           PPECategoryReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPECategory Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.PPECategory.Models;
using PlantSafety.Application.PPE.PPECategory.Repositories;

namespace PlantSafety.Infrastructure.PPE.PPECategory.Persistence;

/// <summary>Dapper-backed repository for PPECategory read-model queries.</summary>
public sealed class PPECategoryReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPECategoryReadDapperRepository> logger)
    : IPPECategoryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPECategoryItemDto>> GetPPECategoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPECategory tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPECategoryItemDto>(
            "[PPE].[usp_PPECategory_GetRange]", parameters, cancellationToken);
    }
}
