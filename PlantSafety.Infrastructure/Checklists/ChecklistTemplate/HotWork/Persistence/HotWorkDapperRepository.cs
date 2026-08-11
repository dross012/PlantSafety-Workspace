// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.HotWork
//  Layer:          Infrastructure
//  File:           HotWorkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HotWork read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.HotWork.Persistence;

/// <summary>Dapper-backed repository for HotWork read-model queries.</summary>
public sealed class HotWorkDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HotWorkDapperRepository> logger)
    : IHotWorkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HotWorkItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for HotWork tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HotWorkItemDto>(
            "[Checklists].[usp_HotWork_GetItems]", parameters, cancellationToken);
    }
}
