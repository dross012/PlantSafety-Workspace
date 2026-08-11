// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Infrastructure
//  File:           PlanningProjectsService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Application service facade for PlanningProjects infrastructure operations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.PlanningProjects.Services;

/// <summary>Application service for PlanningProjects infrastructure operations.</summary>
public sealed class PlanningProjectsService(
    PlanningProjectsRepository repository)
{
    /// <summary>Retrieves a project by identifier.</summary>
    public Task<PlanningProjectsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => repository.GetByIdAsync(id, tenantId, ct);

    /// <summary>Retrieves all projects for the tenant.</summary>
    public Task<IReadOnlyList<PlanningProjectsReadModel>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
        => repository.GetAllAsync(tenantId, ct);
}
