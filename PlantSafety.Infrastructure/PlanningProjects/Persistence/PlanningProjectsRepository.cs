// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Infrastructure
//  File:           PlanningProjectsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for PlanningProjects entities.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.PlanningProjects.Persistence;

/// <summary>Dapper-backed repository for <see cref="PlanningProjectsReadModel"/> queries.</summary>
public sealed class PlanningProjectsRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PlanningProjectsRepository> logger)
{
    /// <summary>Returns the planning project or <c>null</c> if not found.</summary>
    public async Task<PlanningProjectsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById PlanningProject {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ProjectId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PlanningProjectsReadModel>(
            conn, "[PlanningProjects].[usp_Project_GetById]", parameters);
    }

    /// <summary>Returns all projects for the tenant.</summary>
    public async Task<IReadOnlyList<PlanningProjectsReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll PlanningProjects tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<PlanningProjectsReadModel>(
            conn, "[PlanningProjects].[usp_Project_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
