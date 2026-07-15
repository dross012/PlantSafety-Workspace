// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.ConfinedSpace
//  Layer:          Infrastructure
//  File:           ConfinedSpaceDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.ConfinedSpace read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models;
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.ConfinedSpace.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.ConfinedSpace read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ConfinedSpaceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ConfinedSpaceDapperRepository> logger)
    : IConfinedSpaceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConfinedSpaceItemDto>> GetConfinedSpaceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetConfinedSpace for Permits.ConfinedSpace tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ConfinedSpaceItemDto>(
            "[Compliance].[usp_PermitConfinedSpace_GetConfinedSpace]", parameters, cancellationToken);
    }
}
