// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Infrastructure
//  File:           RCADapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RCA read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.RCA.Models;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

namespace PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence;

/// <summary>
/// Dapper-backed repository for RCA read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RCADapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RCADapperRepository> logger)
    : IRCARepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RCAItemDto>> GetRcaRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRcaRecordsAsync for RCA tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RCAItemDto>(
            "[ContinuousImprovement].[usp_RCA_GetRcaRecords]", parameters, cancellationToken);
    }
}
