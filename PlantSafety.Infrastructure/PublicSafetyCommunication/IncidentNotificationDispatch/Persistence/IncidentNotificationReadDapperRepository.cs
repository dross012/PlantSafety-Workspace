// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Infrastructure
//  File:           IncidentNotificationReadDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IncidentNotification Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Models;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Repositories;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.IncidentNotificationDispatch.Persistence;

/// <summary>Dapper-backed repository for IncidentNotification read-model queries.</summary>
public sealed class IncidentNotificationReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentNotificationReadDapperRepository> logger)
    : IIncidentNotificationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentNotificationItemDto>> GetIncidentNotificationStatusAsync(
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentNotificationStatus for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddGuid("IncidentId", incidentId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IncidentNotificationItemDto>(
            "[PublicSafetyCommunication].[usp_IncidentNotification_GetStatus]", parameters, cancellationToken);
    }
}
