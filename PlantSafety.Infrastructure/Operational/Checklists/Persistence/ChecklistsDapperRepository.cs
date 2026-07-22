// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Infrastructure
//  File:           ChecklistsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Checklists read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Checklists.Models;
using PlantSafety.Application.Operational.Checklists.Repositories;

namespace PlantSafety.Infrastructure.Operational.Checklists.Persistence;

/// <summary>Dapper-backed repository for <see cref="ChecklistsItemDto"/> queries.</summary>
public sealed class ChecklistsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ChecklistsDapperRepository> logger)
    : IChecklistsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChecklistsItemDto>> GetChecklistsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetChecklistsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ChecklistsItemDto>(
            "[Operational].[usp_Checklists_List]", parameters, cancellationToken);
    }
}
