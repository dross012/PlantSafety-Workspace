// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.ConfinedSpaceEntry
//  Layer:          Infrastructure
//  File:           ConfinedSpaceEntryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ConfinedSpaceEntry read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Persistence;

/// <summary>Dapper-backed repository for ConfinedSpaceEntry read-model queries.</summary>
public sealed class ConfinedSpaceEntryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ConfinedSpaceEntryDapperRepository> logger)
    : IConfinedSpaceEntryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConfinedSpaceEntryItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for ConfinedSpaceEntry tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ConfinedSpaceEntryItemDto>(
            "[Checklists].[usp_ConfinedSpaceEntry_GetItems]", parameters, cancellationToken);
    }
}
