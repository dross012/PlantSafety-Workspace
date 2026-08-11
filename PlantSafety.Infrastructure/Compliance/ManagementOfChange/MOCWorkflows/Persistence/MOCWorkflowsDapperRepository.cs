// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange.MOCWorkflows
//  Layer:          Infrastructure
//  File:           MOCWorkflowsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ManagementOfChange.MOCWorkflows read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Models;
using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Repositories;

namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.MOCWorkflows.Persistence;

/// <summary>
/// Dapper-backed repository for ManagementOfChange.MOCWorkflows read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class MOCWorkflowsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<MOCWorkflowsDapperRepository> logger)
    : IMOCWorkflowsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MOCWorkflowsItemDto>> GetMOCWorkflowsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMOCWorkflows for ManagementOfChange.MOCWorkflows tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MOCWorkflowsItemDto>(
            "[Compliance].[usp_MOCWorkflows_GetMOCWorkflows]", parameters, cancellationToken);
    }
}
