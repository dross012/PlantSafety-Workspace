// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Cleanup
//  Layer:          Infrastructure
//  File:           CleanupDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Cleanup read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Cleanup.Models;
using PlantSafety.Application.CrossDomain.Cleanup.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Cleanup.Persistence;

/// <summary>Dapper-backed repository for <see cref="CleanupItemDto"/> queries.</summary>
public sealed class CleanupDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<CleanupDapperRepository> logger)
    : ICleanupRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CleanupItemDto>> GetCleanupTasksAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCleanupTasksAsync for Cleanup tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CleanupItemDto>(
            "[CrossDomain].[usp_Cleanup_GetCleanupTasks]", parameters, cancellationToken);
    }
}
