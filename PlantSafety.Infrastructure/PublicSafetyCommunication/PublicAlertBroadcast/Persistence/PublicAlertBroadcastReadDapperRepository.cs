// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Infrastructure
//  File:           PublicAlertBroadcastReadDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PublicAlertBroadcast Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Models;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Repositories;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.PublicAlertBroadcast.Persistence;

/// <summary>Dapper-backed repository for PublicAlertBroadcast read-model queries.</summary>
public sealed class PublicAlertBroadcastReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PublicAlertBroadcastReadDapperRepository> logger)
    : IPublicAlertBroadcastRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PublicAlertBroadcastItemDto>> ListActiveBroadcastsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListActiveBroadcasts for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<PublicAlertBroadcastItemDto>(
            "[PublicSafetyCommunication].[usp_PublicAlertBroadcast_ListActive]", parameters, cancellationToken);
    }
}
