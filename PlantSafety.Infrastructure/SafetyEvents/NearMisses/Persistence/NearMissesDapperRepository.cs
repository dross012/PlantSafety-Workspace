// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Infrastructure
//  File:           NearMissesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the NearMisses read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.NearMisses.Models;
using PlantSafety.Application.SafetyEvents.NearMisses.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.NearMisses.Persistence;

/// <summary>
/// Dapper-backed repository for NearMisses read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class NearMissesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<NearMissesDapperRepository> logger)
    : INearMissesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<NearMissesItemDto>> GetNearMissesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetNearMisses for NearMisses tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<NearMissesItemDto>(
            "[SafetyEvents].[usp_NearMisses_GetNearMisses]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NearMissesDetailDto?> GetByIdAsync(Guid nearMissesId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for NearMisses {NearMissesId} Tenant {TenantId}", nearMissesId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("NearMissesId", nearMissesId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<NearMissesDetailDto>(
            "[SafetyEvents].[usp_NearMisses_GetById]", parameters, cancellationToken);
    }
}
