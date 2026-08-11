// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.Executive
//  Layer:          Infrastructure
//  File:           ExecutiveDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Executive read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.Executive.Models;
using PlantSafety.Application.Analytics.Dashboard.Executive.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.Executive.Persistence;

/// <summary>Dapper-backed repository for <see cref="ExecutiveItemDto"/> queries.</summary>
public sealed class ExecutiveDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ExecutiveDapperRepository> logger)
    : IExecutiveRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExecutiveItemDto>> GetExecutiveSummaryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetExecutiveSummaryAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ExecutiveItemDto>(
            "[Analytics].[usp_Executive_List]", parameters, cancellationToken);
    }
}
