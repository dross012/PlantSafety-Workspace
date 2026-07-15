// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.Excavation
//  Layer:          Infrastructure
//  File:           ExcavationDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.Excavation read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Excavation.Models;
using PlantSafety.Application.Compliance.Permits.Excavation.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.Excavation.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.Excavation read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ExcavationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ExcavationDapperRepository> logger)
    : IExcavationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExcavationItemDto>> GetExcavationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetExcavation for Permits.Excavation tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ExcavationItemDto>(
            "[Compliance].[usp_PermitExcavation_GetExcavation]", parameters, cancellationToken);
    }
}
