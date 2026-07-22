// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Infrastructure
//  File:           WorkOrderPriorityReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkOrderPriority Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.WorkOrderPriority.Models;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.WorkOrderPriority.Persistence;

/// <summary>Dapper-backed repository for WorkOrderPriority read-model queries.</summary>
public sealed class WorkOrderPriorityReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkOrderPriorityReadDapperRepository> logger)
    : IWorkOrderPriorityRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkOrderPriorityItemDto>> GetWorkOrderPriorityAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for WorkOrderPriority tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WorkOrderPriorityItemDto>(
            "[Scheduling].[usp_WorkOrderPriority_GetRange]", parameters, cancellationToken);
    }
}
