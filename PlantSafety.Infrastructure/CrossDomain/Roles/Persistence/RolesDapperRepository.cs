// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Roles
//  Layer:          Infrastructure
//  File:           RolesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Roles read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Roles.Models;
using PlantSafety.Application.CrossDomain.Roles.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Roles.Persistence;

/// <summary>Dapper-backed repository for <see cref="RolesItemDto"/> queries.</summary>
public sealed class RolesDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RolesDapperRepository> logger)
    : IRolesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RolesItemDto>> GetRolesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRolesAsync for Roles tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RolesItemDto>(
            "[CrossDomain].[usp_Roles_GetRoles]", parameters, cancellationToken);
    }
}
