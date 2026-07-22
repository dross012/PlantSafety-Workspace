// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           IDashboardRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Dashboard aggregate root. Implemented in
//                  the Infrastructure layer (Dapper/stored-procedure-only; not included in this
//                  Domain+Application-only generation pass).
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardSpecification = PlantSafety.Domain.Dashboards.Dashboard.Specifications.DashboardSpecification;

namespace PlantSafety.Domain.Dashboards.Dashboard.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.Dashboard"/> aggregate root.</summary>
public interface IDashboardRepository
{
    /// <summary>Retrieves a Dashboard aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.Dashboard?> GetByIdAsync(DashboardId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Dashboard aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.Dashboard>> ListAsync(DashboardSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Dashboard aggregate.</summary>
    Task AddAsync(Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Dashboard aggregate.</summary>
    Task UpdateAsync(Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default);
}
