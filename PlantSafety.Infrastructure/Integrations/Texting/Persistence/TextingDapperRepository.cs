// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Texting
//  Layer:          Infrastructure
//  File:           TextingDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the root-level Integrations.Texting read-model, backing
//                  PlantSafety.Application.Integrations.Texting.Repositories.ITextingRepository.
//                  Distinct from NotificationsTextingDapperRepository, which backs the separate
//                  Integrations.Notifications.Texting.ITextingRepository interface.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Texting.Models;
using PlantSafety.Application.Integrations.Texting.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Texting.Persistence;

/// <summary>
/// Dapper-backed repository for the root-level Integrations.Texting read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class TextingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<TextingDapperRepository> logger)
    : ITextingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TextingItemDto>> GetTextMessagesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTextMessages for Integrations.Texting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TextingItemDto>(
            "[Integrations].[usp_Texting_GetTextMessages]", parameters, cancellationToken);
    }
}
