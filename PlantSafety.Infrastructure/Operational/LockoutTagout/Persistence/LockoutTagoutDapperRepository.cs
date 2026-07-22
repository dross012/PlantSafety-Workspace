// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.LockoutTagout
//  Layer:          Infrastructure
//  File:           LockoutTagoutDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LockoutTagout read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.LockoutTagout.Models;
using PlantSafety.Application.Operational.LockoutTagout.Repositories;

namespace PlantSafety.Infrastructure.Operational.LockoutTagout.Persistence;

/// <summary>Dapper-backed repository for <see cref="LockoutTagoutItemDto"/> queries.</summary>
public sealed class LockoutTagoutDapperRepository(
    ISqlConnectionFactory connectionFactory,
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
        logger.LogDebug("GetLockoutTagoutAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LockoutTagoutItemDto>(
            "[Operational].[usp_LockoutTagout_List]", parameters, cancellationToken);
    }
}
