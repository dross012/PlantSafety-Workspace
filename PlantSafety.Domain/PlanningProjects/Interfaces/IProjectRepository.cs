// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           IProjectRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the Project aggregate root. NOTE:
//                  this simple name "IProjectRepository" already exists in a DIFFERENT namespace
//                  (PlantSafety.Application.Features.PlanningProjects.Handlers.Projects, an
//                  unrelated, pre-existing, LIVE generic project-tracking vertical) -- same
//                  disambiguation-by-namespace pattern used throughout this codebase (e.g.
//                  IRootCauseAnalysisRepository exists in both Domain and Application namespaces).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Interfaces;

/// <summary>Domain repository interface for the Project aggregate root.</summary>
public interface IProjectRepository
{
    /// <summary>
    /// Retrieves a Project aggregate by its identity, scoped to <paramref name="tenantId"/>, or
    /// <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.PlanningProjects.Aggregates.Project?> GetByIdAsync(
        ProjectId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Project aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PlanningProjects.Aggregates.Project aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Project aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.PlanningProjects.Aggregates.Project aggregate, string? eTag, CancellationToken cancellationToken = default);
}
