// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Infrastructure
//  File:           DataLakeDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DataLake read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.DataLake.Models;
using PlantSafety.Application.Intelligence.DataLake.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.DataLake.Persistence;

/// <summary>
/// Dapper-backed repository for DataLake read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DataLakeDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<DataLakeDapperRepository> logger)
    : IDataLakeRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DataLakeItemDto>> GetDataLakeRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDataLakeRecordsAsync for DataLake tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DataLakeItemDto>(
            "[Intelligence].[usp_DataLake_GetDataLakeRecords]", parameters, cancellationToken);
    }
}
