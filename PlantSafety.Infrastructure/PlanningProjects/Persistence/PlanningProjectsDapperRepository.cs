// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.PlanningProjects
//  Layer:          Infrastructure
//  File:           PlanningProjectsDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PlanningProjects read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PlanningProjects.PlanningProjects.Models;
using PlantSafety.Application.PlanningProjects.PlanningProjects.Repositories;

namespace PlantSafety.Infrastructure.PlanningProjects.Persistence;

/// <summary>
/// Dapper-backed repository for PlanningProjects read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class PlanningProjectsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PlanningProjectsDapperRepository> logger)
    : IPlanningProjectsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlanningProjectItemDto>> GetProjectsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetProjectsAsync for PlanningProjects tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PlanningProjectItemDto>(
            "[PlanningProjects].[usp_PlanningProjects_GetProjects]", parameters, cancellationToken);
    }
}
