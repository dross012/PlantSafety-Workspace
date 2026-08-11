// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Administration
//  Layer:          Infrastructure
//  File:           AdministrationDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Administration read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Administration.Models;
using PlantSafety.Application.CrossDomain.Administration.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Administration.Persistence;

/// <summary>Dapper-backed repository for <see cref="AdministrationItemDto"/> queries.</summary>
public sealed class AdministrationDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AdministrationDapperRepository> logger)
    : IAdministrationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AdministrationItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRecordsAsync for Administration tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AdministrationItemDto>(
            "[CrossDomain].[usp_Administration_GetRecords]", parameters, cancellationToken);
    }
}
