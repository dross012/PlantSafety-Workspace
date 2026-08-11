// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.ColdWork
//  Layer:          Infrastructure
//  File:           ColdWorkDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.ColdWork read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.ColdWork.Models;
using PlantSafety.Application.Compliance.Permits.ColdWork.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.ColdWork.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.ColdWork read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ColdWorkDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ColdWorkDapperRepository> logger)
    : IColdWorkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ColdWorkItemDto>> GetColdWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetColdWork for Permits.ColdWork tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ColdWorkItemDto>(
            "[Compliance].[usp_PermitColdWork_GetColdWork]", parameters, cancellationToken);
    }
}
