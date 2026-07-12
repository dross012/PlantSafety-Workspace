// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.PlanningProjects.Projects
//  Layer:          Infrastructure
//  File:           ProjectDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyProject aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;

namespace PlantSafety.Infrastructure.Features.PlanningProjects.Projects.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>SafetyProject</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ProjectDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<ProjectDapperRepository> logger)
    : IProjectRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid projectId,
        Guid tenantId,
        Guid siteId,
        string name,
        string description,
        string priority,
        Guid projectOwnerId,
        Guid? projectManagerId,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset plannedEndUtc,
        decimal? plannedBudget,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create SafetyProject {ProjectId} for tenant {TenantId}", projectId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ProjectId", projectId)
            .AddGuid("SiteId", siteId)
            .AddString("Name", name, 200)
            .AddString("Description", description)
            .AddString("Priority", priority, 50)
            .AddGuid("ProjectOwnerId", projectOwnerId)
            .AddGuid("ProjectManagerId", projectManagerId)
            .AddDateTime("PlannedStartUtc", plannedStartUtc.UtcDateTime)
            .AddDateTime("PlannedEndUtc", plannedEndUtc.UtcDateTime)
            .AddDecimal("PlannedBudget", plannedBudget)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PlanningProjects].[usp_Project_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<ProjectReadModel?> GetByIdAsync(Guid tenantId, Guid projectId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get SafetyProject {ProjectId} for tenant {TenantId}", projectId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ProjectId", projectId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ProjectReadModel>(
            "[PlanningProjects].[usp_Project_GetById]", parameters, cancellationToken);
    }
}
