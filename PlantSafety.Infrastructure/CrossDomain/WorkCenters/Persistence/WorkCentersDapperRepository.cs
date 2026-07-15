// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.WorkCenters
//  Layer:          Infrastructure
//  File:           WorkCentersDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkCenters read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.WorkCenters.Models;
using PlantSafety.Application.CrossDomain.WorkCenters.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.WorkCenters.Persistence;

/// <summary>Dapper-backed repository for <see cref="WorkCentersItemDto"/> queries.</summary>
public sealed class WorkCentersDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<WorkCentersDapperRepository> logger)
    : IWorkCentersRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkCentersItemDto>> GetWorkCentersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWorkCentersAsync for WorkCenters tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkCentersItemDto>(
            "[CrossDomain].[usp_WorkCenters_GetWorkCenters]", parameters, cancellationToken);
    }
}
