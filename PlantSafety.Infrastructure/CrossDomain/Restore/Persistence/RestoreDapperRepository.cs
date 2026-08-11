// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Restore
//  Layer:          Infrastructure
//  File:           RestoreDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Restore read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Restore.Models;
using PlantSafety.Application.CrossDomain.Restore.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Restore.Persistence;

/// <summary>Dapper-backed repository for <see cref="RestoreItemDto"/> queries.</summary>
public sealed class RestoreDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RestoreDapperRepository> logger)
    : IRestoreRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RestoreItemDto>> GetRestoreJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRestoreJobsAsync for Restore tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RestoreItemDto>(
            "[CrossDomain].[usp_Restore_GetRestoreJobs]", parameters, cancellationToken);
    }
}
