// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Permissions
//  Layer:          Infrastructure
//  File:           PermissionsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permissions read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Permissions.Models;
using PlantSafety.Application.CrossDomain.Permissions.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Permissions.Persistence;

/// <summary>Dapper-backed repository for <see cref="PermissionsItemDto"/> queries.</summary>
public sealed class PermissionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PermissionsDapperRepository> logger)
    : IPermissionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermissionsItemDto>> GetPermissionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPermissionsAsync for Permissions tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermissionsItemDto>(
            "[CrossDomain].[usp_Permissions_GetPermissions]", parameters, cancellationToken);
    }
}
