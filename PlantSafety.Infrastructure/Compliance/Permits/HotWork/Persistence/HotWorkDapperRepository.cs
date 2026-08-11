// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.HotWork
//  Layer:          Infrastructure
//  File:           HotWorkDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.HotWork read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.HotWork.Models;
using PlantSafety.Application.Compliance.Permits.HotWork.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.HotWork.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.HotWork read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class HotWorkDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HotWorkDapperRepository> logger)
    : IHotWorkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HotWorkItemDto>> GetHotWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHotWork for Permits.HotWork tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HotWorkItemDto>(
            "[Compliance].[usp_PermitHotWork_GetHotWork]", parameters, cancellationToken);
    }
}
