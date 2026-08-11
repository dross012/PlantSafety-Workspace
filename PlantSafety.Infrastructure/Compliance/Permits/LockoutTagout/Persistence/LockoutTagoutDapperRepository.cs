// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.LockoutTagout
//  Layer:          Infrastructure
//  File:           LockoutTagoutDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.LockoutTagout read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.LockoutTagout.Models;
using PlantSafety.Application.Compliance.Permits.LockoutTagout.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.LockoutTagout.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.LockoutTagout read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class LockoutTagoutDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<LockoutTagoutDapperRepository> logger)
    : ILockoutTagoutRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LockoutTagoutItemDto>> GetLockoutTagoutAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLockoutTagout for Permits.LockoutTagout tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LockoutTagoutItemDto>(
            "[Compliance].[usp_PermitLockoutTagout_GetLockoutTagout]", parameters, cancellationToken);
    }
}
