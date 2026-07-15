// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.ReadModel
//  Layer:          Infrastructure
//  File:           ReadModelDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ReadModel read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.ReadModel.Models;
using PlantSafety.Application.Analytics.Dashboard.ReadModel.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.ReadModel.Persistence;

/// <summary>Dapper-backed repository for <see cref="ReadModelItemDto"/> queries.</summary>
public sealed class ReadModelDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ReadModelDapperRepository> logger)
    : IReadModelRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReadModelItemDto>> GetDashboardRowsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDashboardRowsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ReadModelItemDto>(
            "[Analytics].[usp_ReadModel_List]", parameters, cancellationToken);
    }
}
