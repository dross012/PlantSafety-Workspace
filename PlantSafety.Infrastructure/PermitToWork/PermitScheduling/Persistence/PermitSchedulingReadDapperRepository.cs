// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Infrastructure
//  File:           PermitSchedulingReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitScheduling Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PermitToWork.PermitScheduling.Models;
using PlantSafety.Application.PermitToWork.PermitScheduling.Repositories;

namespace PlantSafety.Infrastructure.PermitToWork.PermitScheduling.Persistence;

/// <summary>Dapper-backed repository for PermitScheduling read-model queries.</summary>
public sealed class PermitSchedulingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermitSchedulingReadDapperRepository> logger)
    : IPermitSchedulingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitSchedulingItemDto>> GetPermitSchedulingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PermitScheduling tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitSchedulingItemDto>(
            "[PermitToWork].[usp_PermitScheduling_GetRange]", parameters, cancellationToken);
    }
}
