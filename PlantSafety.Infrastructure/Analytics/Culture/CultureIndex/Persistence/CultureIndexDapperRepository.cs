// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture.CultureIndex
//  Layer:          Infrastructure
//  File:           CultureIndexDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CultureIndex read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.CultureIndex.Models;
using PlantSafety.Application.Analytics.Culture.CultureIndex.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Culture.CultureIndex.Persistence;

/// <summary>Dapper-backed repository for <see cref="CultureIndexItemDto"/> queries.</summary>
public sealed class CultureIndexDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CultureIndexDapperRepository> logger)
    : ICultureIndexRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CultureIndexItemDto>> GetCultureIndexSnapshotsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCultureIndexSnapshotsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CultureIndexItemDto>(
            "[Analytics].[usp_CultureIndex_List]", parameters, cancellationToken);
    }
}
