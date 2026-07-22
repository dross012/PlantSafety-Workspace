// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Notifications.Texting
//  Layer:          Infrastructure
//  File:           NotificationsTextingDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Notifications.Texting read-model, backing
//                  Integrations.Notifications.Texting.Repositories.ITextingRepository. Distinct from
//                  TextingDapperRepository, which backs the separate root-level
//                  Integrations.Texting.ITextingRepository interface.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Notifications.Texting.Models;
using PlantSafety.Application.Integrations.Notifications.Texting.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Notifications.Texting.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Notifications.Texting read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class NotificationsTextingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<NotificationsTextingDapperRepository> logger)
    : ITextingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TextingItemDto>> GetTextingDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTextingDeliveries for Integrations.Notifications.Texting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TextingItemDto>(
            "[Integrations].[usp_NotificationsTexting_GetTextingDeliveries]", parameters, cancellationToken);
    }
}
