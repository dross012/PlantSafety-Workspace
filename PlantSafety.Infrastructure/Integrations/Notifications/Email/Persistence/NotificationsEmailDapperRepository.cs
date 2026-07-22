// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Notifications.Email
//  Layer:          Infrastructure
//  File:           NotificationsEmailDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Notifications.Email read-model, backing
//                  Integrations.Notifications.Email.Repositories.IEmailRepository. Distinct from
//                  EmailDapperRepository, which backs the separate root-level
//                  Integrations.Email.IEmailRepository interface.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Notifications.Email.Models;
using PlantSafety.Application.Integrations.Notifications.Email.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Notifications.Email.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Notifications.Email read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class NotificationsEmailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<NotificationsEmailDapperRepository> logger)
    : IEmailRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmailItemDto>> GetEmailDeliveriesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmailDeliveries for Integrations.Notifications.Email tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmailItemDto>(
            "[Integrations].[usp_NotificationsEmail_GetEmailDeliveries]", parameters, cancellationToken);
    }
}
