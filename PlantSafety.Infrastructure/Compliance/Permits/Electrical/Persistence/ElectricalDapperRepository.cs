// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.Electrical
//  Layer:          Infrastructure
//  File:           ElectricalDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.Electrical read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Electrical.Models;
using PlantSafety.Application.Compliance.Permits.Electrical.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.Electrical.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.Electrical read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ElectricalDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ElectricalDapperRepository> logger)
    : IElectricalRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ElectricalItemDto>> GetElectricalAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetElectrical for Permits.Electrical tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ElectricalItemDto>(
            "[Compliance].[usp_PermitElectrical_GetElectrical]", parameters, cancellationToken);
    }
}
