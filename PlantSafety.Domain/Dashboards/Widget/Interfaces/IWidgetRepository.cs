// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           IWidgetRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Widget aggregate root. Implemented in the
//                  Infrastructure layer (Dapper/stored-procedure-only; not included in this
//                  Domain+Application-only generation pass).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Specifications;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.Widget"/> aggregate root.</summary>
public interface IWidgetRepository
{
    /// <summary>Retrieves a Widget aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.Widget?> GetByIdAsync(WidgetId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Widget aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.Widget>> ListAsync(WidgetSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Widget aggregate.</summary>
    Task AddAsync(Aggregates.Widget aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Widget aggregate.</summary>
    Task UpdateAsync(Aggregates.Widget aggregate, CancellationToken cancellationToken = default);
}
