// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           IAccidentRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Accident aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Specifications;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Interfaces;

/// <summary>Domain repository interface for the Accident aggregate root.</summary>
public interface IAccidentRepository
{
    /// <summary>Retrieves a Accident aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Accident?> GetByIdAsync(AccidentId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Accident aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Accident>> ListAsync(AccidentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Accident aggregate.</summary>
    Task AddAsync(Accident aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Accident aggregate.</summary>
    Task UpdateAsync(Accident aggregate, CancellationToken cancellationToken = default);
}