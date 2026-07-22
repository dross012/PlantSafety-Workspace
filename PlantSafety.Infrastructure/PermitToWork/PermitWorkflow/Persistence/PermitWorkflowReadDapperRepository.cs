// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Infrastructure
//  File:           PermitWorkflowReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitWorkflow Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PermitToWork.PermitWorkflow.Models;
using PlantSafety.Application.PermitToWork.PermitWorkflow.Repositories;

namespace PlantSafety.Infrastructure.PermitToWork.PermitWorkflow.Persistence;

/// <summary>Dapper-backed repository for PermitWorkflow read-model queries.</summary>
public sealed class PermitWorkflowReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermitWorkflowReadDapperRepository> logger)
    : IPermitWorkflowRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermitWorkflowItemDto>> GetPermitWorkflowAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PermitWorkflow tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitWorkflowItemDto>(
            "[PermitToWork].[usp_PermitWorkflow_GetRange]", parameters, cancellationToken);
    }
}
