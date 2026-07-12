// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           IEngagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Engagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="Engagement"/> aggregate root.</summary>
public interface IEngagementRepository
{
    /// <summary>Retrieves a Engagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement?> GetByIdAsync(EngagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Engagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement>> ListAsync(EngagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Engagement aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Engagement aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, CancellationToken cancellationToken = default);
}
