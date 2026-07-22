// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Email
//  Layer:          Infrastructure
//  File:           EmailDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the root-level Integrations.Email read-model, backing
//                  PlantSafety.Application.Integrations.Email.Repositories.IEmailRepository.
//                  Distinct from NotificationsEmailDapperRepository, which backs the separate
//                  Integrations.Notifications.Email.IEmailRepository interface.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Email.Models;
using PlantSafety.Application.Integrations.Email.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Email.Persistence;

/// <summary>
/// Dapper-backed repository for the root-level Integrations.Email read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class EmailDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EmailDapperRepository> logger)
    : IEmailRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmailItemDto>> GetEmailsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmails for Integrations.Email tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmailItemDto>(
            "[Integrations].[usp_Email_GetEmails]", parameters, cancellationToken);
    }
}
