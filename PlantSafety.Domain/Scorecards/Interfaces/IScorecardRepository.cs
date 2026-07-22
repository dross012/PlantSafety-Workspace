// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           IScorecardRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Scorecard aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scorecards.Aggregates;
using PlantSafety.Domain.Scorecards.Specifications;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Interfaces;

/// <summary>Domain repository interface for the Scorecard aggregate root.</summary>
public interface IScorecardRepository
{
    /// <summary>Retrieves a Scorecard aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Scorecard?> GetByIdAsync(ScorecardId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Scorecard aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Scorecard>> ListAsync(ScorecardSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Scorecard aggregate.</summary>
    Task AddAsync(Scorecard aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Scorecard aggregate.</summary>
    Task UpdateAsync(Scorecard aggregate, CancellationToken cancellationToken = default);
}