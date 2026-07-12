// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           IDashboardRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Dashboard aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Interfaces;

/// <summary>Domain repository interface for the <see cref="Dashboard"/> aggregate root.</summary>
public interface IDashboardRepository
{
    /// <summary>Retrieves a Dashboard aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard?> GetByIdAsync(DashboardId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Dashboard aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard>> ListAsync(DashboardSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Dashboard aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Dashboard aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default);
}
