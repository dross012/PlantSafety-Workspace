// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.Lifting
//  Layer:          Infrastructure
//  File:           LiftingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.Lifting read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Lifting.Models;
using PlantSafety.Application.Compliance.Permits.Lifting.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.Lifting.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.Lifting read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class LiftingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<LiftingDapperRepository> logger)
    : ILiftingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LiftingItemDto>> GetLiftingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLifting for Permits.Lifting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LiftingItemDto>(
            "[Compliance].[usp_PermitLifting_GetLifting]", parameters, cancellationToken);
    }
}
