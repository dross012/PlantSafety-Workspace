// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           IRouteDefinitionReadRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the RouteDefinition read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.ProcessManagement.Routing.Repositories.IRouteDefinitionRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.ProcessManagement.Repositories.IProcessDefinitionRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Repositories;

/// <summary>Read-only repository for RouteDefinition data. Implemented in the Infrastructure layer.</summary>
public interface IRouteDefinitionReadRepository
{
    /// <summary>
    /// Returns a deterministically ordered, paginated page of routes for the tenant, optionally
    /// filtered by a free-text search term matched against Name/PlantName.
    /// </summary>
    Task<(IReadOnlyList<RouteDefinitionSummaryDto> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the RouteDefinition detail read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<RouteDefinitionDetailDto?> GetByIdAsync(Guid tenantId, Guid routeDefinitionId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the ordered step list for a route, with each step's domain-computed status, or
    /// <see langword="null"/> if the route is not found for the tenant.
    /// </summary>
    Task<IReadOnlyList<RouteStepItemDto>?> GetStepsAsync(Guid tenantId, Guid routeDefinitionId, CancellationToken cancellationToken);
}
