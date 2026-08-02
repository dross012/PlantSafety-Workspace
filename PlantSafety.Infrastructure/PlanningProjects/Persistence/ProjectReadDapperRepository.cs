// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Infrastructure
//  File:           ProjectReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProjectReadRepository (Application-layer
//                  read-side repository for the Project aggregate's by-id detail query). Reuses
//                  usp_PlanningProject_GetById's two result sets (header + milestones) directly
//                  via QueryMultipleAsync, avoiding domain-aggregate reconstruction. Mirrors
//                  PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence
//                  .RootCauseAnalysisDapperRepository's read-side precedent.
//                  Additive-only evolution.
// ============================================================================
using System.Data;
using Dapper;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Repositories;

namespace PlantSafety.Infrastructure.PlanningProjects.Persistence;

/// <summary>Dapper-backed read-side repository for the Project aggregate's by-id detail query.</summary>
public sealed class ProjectReadDapperRepository(
    ISqlConnectionFactory connectionFactory,
    ILogger<ProjectReadDapperRepository> logger)
    : IProjectReadRepository
{
    /// <inheritdoc/>
    public async Task<ProjectDetailDto?> GetByIdAsync(Guid projectId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for Project {ProjectId} Tenant {TenantId}", projectId, tenantId);

        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProjectId", projectId)
            .AddGuid("TenantId", tenantId)
            .Build();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                "[PlanningProjects].[usp_PlanningProject_GetById]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var header = await multi.ReadSingleOrDefaultAsync<ProjectHeaderRow>();
        if (header is null) return null;

        var milestones = (await multi.ReadAsync<ProjectMilestoneDto>()).ToList();

        return new ProjectDetailDto
        {
            ProjectId = header.ProjectId,
            TenantId = header.TenantId,
            SiteId = header.SiteId,
            Name = header.Name,
            Status = header.Status,
            TargetCompletionUtc = header.TargetCompletionUtc,
            OwnerId = header.OwnerId,
            CreatedAtUtc = header.CreatedAtUtc,
            CreatedBy = header.CreatedBy,
            Milestones = milestones,
            ETag = header.ETag,
        };
    }

    /// <summary>Flat header row shape returned by <c>usp_PlanningProject_GetById</c>'s first result set.</summary>
    private sealed record ProjectHeaderRow
    {
        public required Guid ProjectId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Name { get; init; }
        public required string Status { get; init; }
        public required DateTime TargetCompletionUtc { get; init; }
        public required Guid OwnerId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public string? ETag { get; init; }
    }
}
