// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.WorkingAtHeights
//  Layer:          Infrastructure
//  File:           WorkingAtHeightsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkingAtHeights read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.WorkingAtHeights.Persistence;

/// <summary>Dapper-backed repository for WorkingAtHeights read-model queries.</summary>
public sealed class WorkingAtHeightsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkingAtHeightsDapperRepository> logger)
    : IWorkingAtHeightsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkingAtHeightsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for WorkingAtHeights tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkingAtHeightsItemDto>(
            "[Checklists].[usp_WorkingAtHeights_GetItems]", parameters, cancellationToken);
    }
}
