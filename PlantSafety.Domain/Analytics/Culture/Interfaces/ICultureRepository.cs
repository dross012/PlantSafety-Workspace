// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           ICultureRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Culture aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Interfaces;

/// <summary>Domain repository interface for the <see cref="Culture"/> aggregate root.</summary>
public interface ICultureRepository
{
    /// <summary>Retrieves a Culture aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Culture?> GetByIdAsync(CultureId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Culture aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Culture>> ListAsync(CultureSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Culture aggregate.</summary>
    Task AddAsync(Culture aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Culture aggregate.</summary>
    Task UpdateAsync(Culture aggregate, CancellationToken cancellationToken = default);
}
