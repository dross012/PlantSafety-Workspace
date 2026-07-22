// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Notifications.Internal
//  Layer:          Infrastructure
//  File:           InternalDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Notifications.Internal read-model.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Notifications.Internal.Models;
using PlantSafety.Application.Integrations.Notifications.Internal.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Notifications.Internal.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Notifications.Internal read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class InternalDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<InternalDapperRepository> logger)
    : IInternalRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<InternalItemDto>> GetInternalNotificationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInternalNotifications for Integrations.Notifications.Internal tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<InternalItemDto>(
            "[Integrations].[usp_NotificationsInternal_GetInternalNotifications]", parameters, cancellationToken);
    }
}
