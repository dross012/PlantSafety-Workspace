// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           IProjectReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Project aggregate's by-id detail
//                  query. Named "IProjectReadRepository" (not "IProjectRepository") to avoid
//                  colliding with both the pre-existing, unrelated
//                  Application.Features.PlanningProjects.Handlers.Projects.IProjectRepository
//                  (LIVE generic project-tracking vertical) and the Domain-layer write-side
//                  PlantSafety.Domain.PlanningProjects.Interfaces.IProjectRepository. Implemented
//                  in the Infrastructure layer.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Repositories;

/// <summary>Read-only repository for the Project aggregate's by-id detail query.</summary>
public interface IProjectReadRepository
{
    /// <summary>
    /// Returns the full Project read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<ProjectDetailDto?> GetByIdAsync(Guid projectId, Guid tenantId, CancellationToken cancellationToken);
}
