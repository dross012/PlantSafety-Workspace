// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           IRouteVersionSnapshotReadRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the RouteVersionSnapshot read-model.
//                  Implemented in the Infrastructure layer. Backs the Version History and Diff
//                  Viewer pages. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Application.ProcessManagement.Routing.Repositories;

/// <summary>Read-only repository for RouteVersionSnapshot data. Implemented in the Infrastructure layer.</summary>
public interface IRouteVersionSnapshotReadRepository
{
    /// <summary>Returns a deterministically ordered, paginated page of version history for a route.</summary>
    Task<(IReadOnlyList<Models.RouteVersionSummaryDto> Items, int TotalCount)> GetPagedForRouteAsync(
        Guid tenantId, Guid routeDefinitionId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the full ordered step-snapshot content for a single version, scoped to the given
    /// tenant and route, or <see langword="null"/> if not found.
    /// </summary>
    Task<IReadOnlyList<RouteStepSnapshot>?> GetStepSnapshotsAsync(
        Guid tenantId, Guid routeDefinitionId, Guid versionId, CancellationToken cancellationToken);
}
