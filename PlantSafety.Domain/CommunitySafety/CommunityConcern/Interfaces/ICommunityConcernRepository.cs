// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           ICommunityConcernRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer write repository abstraction for the CommunityConcern aggregate.
//                  Implemented in Infrastructure (Dapper, stored procedures only). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Specifications;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;

/// <summary>Write repository abstraction for the CommunityConcern aggregate.</summary>
public interface ICommunityConcernRepository
{
    /// <summary>Loads a CommunityConcern aggregate by its identity, or null if not found.</summary>
    Task<Agg?> GetByIdAsync(CommunityConcernId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all CommunityConcern aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Agg>> ListAsync(CommunityConcernSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a newly-created CommunityConcern aggregate.</summary>
    Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CommunityConcern aggregate.</summary>
    Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default);
}
