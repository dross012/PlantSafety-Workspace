// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Updates
//  Layer:          Infrastructure
//  File:           UpdatesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Updates read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Updates.Models;
using PlantSafety.Application.CrossDomain.Updates.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Updates.Persistence;

/// <summary>Dapper-backed repository for <see cref="UpdatesItemDto"/> queries.</summary>
public sealed class UpdatesDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<UpdatesDapperRepository> logger)
    : IUpdatesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<UpdatesItemDto>> GetUpdatesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetUpdatesAsync for Updates tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<UpdatesItemDto>(
            "[CrossDomain].[usp_Updates_GetUpdates]", parameters, cancellationToken);
    }
}
