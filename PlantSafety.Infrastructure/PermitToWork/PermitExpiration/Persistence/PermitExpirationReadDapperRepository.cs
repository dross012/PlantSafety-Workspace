// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Infrastructure
//  File:           PermitExpirationReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitExpiration Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PermitToWork.PermitExpiration.Models;
using PlantSafety.Application.PermitToWork.PermitExpiration.Repositories;

namespace PlantSafety.Infrastructure.PermitToWork.PermitExpiration.Persistence;

/// <summary>Dapper-backed repository for PermitExpiration read-model queries.</summary>
public sealed class PermitExpirationReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermitExpirationReadDapperRepository> logger)
    : IPermitExpirationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitExpirationItemDto>> GetPermitExpirationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PermitExpiration tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitExpirationItemDto>(
            "[PermitToWork].[usp_PermitExpiration_GetRange]", parameters, cancellationToken);
    }
}
