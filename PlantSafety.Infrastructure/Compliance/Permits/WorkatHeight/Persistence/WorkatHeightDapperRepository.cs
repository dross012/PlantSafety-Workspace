// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.WorkatHeight
//  Layer:          Infrastructure
//  File:           WorkatHeightDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.WorkatHeight read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.WorkatHeight.Models;
using PlantSafety.Application.Compliance.Permits.WorkatHeight.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.WorkatHeight.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.WorkatHeight read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class WorkatHeightDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkatHeightDapperRepository> logger)
    : IWorkatHeightRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkatHeightItemDto>> GetWorkatHeightAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWorkatHeight for Permits.WorkatHeight tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkatHeightItemDto>(
            "[Compliance].[usp_PermitWorkatHeight_GetWorkatHeight]", parameters, cancellationToken);
    }
}
