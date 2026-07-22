// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.Systems
//  Layer:          Infrastructure
//  File:           SystemsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Systems read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Models;
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Systems.Persistence;

/// <summary>Dapper-backed repository for Systems read-model queries.</summary>
public sealed class SystemsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SystemsDapperRepository> logger)
    : ISystemsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SystemsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for Systems tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SystemsItemDto>(
            "[Auditing].[usp_Systems_GetItems]", parameters, cancellationToken);
    }
}
