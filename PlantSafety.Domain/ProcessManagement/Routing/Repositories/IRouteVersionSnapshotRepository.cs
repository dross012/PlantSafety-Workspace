// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           IRouteVersionSnapshotRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RouteVersionSnapshot aggregate root.
//                  Genuinely new -- ProcessDefinition has no persisted version-snapshot concept.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Domain.ProcessManagement.Routing.Repositories;

/// <summary>Domain repository interface for the <see cref="RouteVersionSnapshot"/> aggregate root.</summary>
public interface IRouteVersionSnapshotRepository
{
    /// <summary>Persists a newly captured RouteVersionSnapshot.</summary>
    Task AddAsync(RouteVersionSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the most recently captured snapshot for a route, scoped to the given tenant, or <see langword="null"/> if none exists.</summary>
    Task<RouteVersionSnapshot?> GetLatestForRouteAsync(RouteDefinitionId routeDefinitionId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a status change (Approve/Reject) to an existing snapshot.</summary>
    Task UpdateStatusAsync(RouteVersionSnapshot snapshot, CancellationToken cancellationToken = default);
}
