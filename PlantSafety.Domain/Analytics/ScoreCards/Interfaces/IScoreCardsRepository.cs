// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           IScoreCardsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ScoreCards aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Interfaces;

/// <summary>Domain repository interface for the <see cref="ScoreCards"/> aggregate root.</summary>
public interface IScoreCardsRepository
{
    /// <summary>Retrieves a ScoreCards aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards?> GetByIdAsync(ScoreCardsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ScoreCards aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards>> ListAsync(ScoreCardsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ScoreCards aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ScoreCards aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards aggregate, CancellationToken cancellationToken = default);
}
