// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Users
//  Layer:          Infrastructure
//  File:           UsersDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Users read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Users.Models;
using PlantSafety.Application.CrossDomain.Users.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Users.Persistence;

/// <summary>Dapper-backed repository for <see cref="UsersItemDto"/> queries.</summary>
public sealed class UsersDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<UsersDapperRepository> logger)
    : IUsersRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<UsersItemDto>> GetUsersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetUsersAsync for Users tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<UsersItemDto>(
            "[CrossDomain].[usp_Users_GetUsers]", parameters, cancellationToken);
    }
}
