// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           IRouteDefinitionRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RouteDefinition aggregate root. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Repositories.IProcessDefinitionRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Domain.ProcessManagement.Routing.Repositories;

/// <summary>Domain repository interface for the <see cref="RouteDefinition"/> aggregate root.</summary>
public interface IRouteDefinitionRepository
{
    /// <summary>Retrieves a RouteDefinition aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<RouteDefinition?> GetByIdAsync(RouteDefinitionId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RouteDefinition aggregate (and its initial RouteVersionSnapshot).</summary>
    Task AddAsync(RouteDefinition aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RouteDefinition aggregate, enforcing optimistic concurrency via <paramref name="eTag"/>.</summary>
    Task UpdateAsync(RouteDefinition aggregate, string? eTag, CancellationToken cancellationToken = default);
}
