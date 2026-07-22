// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Auditing
//  Layer:          Infrastructure
//  File:           AuditingDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Auditing read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Auditing.Models;
using PlantSafety.Application.Integrations.Auditing.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Auditing.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Auditing read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AuditingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AuditingDapperRepository> logger)
    : IAuditingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditingItemDto>> GetLogsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLogs for Integrations.Auditing tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AuditingItemDto>(
            "[Integrations].[usp_Auditing_GetLogs]", parameters, cancellationToken);
    }
}
