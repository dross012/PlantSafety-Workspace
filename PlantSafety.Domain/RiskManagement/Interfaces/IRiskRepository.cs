// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           IRiskRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Risk aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Specifications;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Interfaces;

/// <summary>Domain repository interface for the Risk aggregate root.</summary>
public interface IRiskRepository
{
    /// <summary>Retrieves a Risk aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Risk?> GetByIdAsync(RiskId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Risk aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Risk>> ListAsync(RiskSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Risk aggregate.</summary>
    Task AddAsync(Risk aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Risk aggregate.</summary>
    Task UpdateAsync(Risk aggregate, CancellationToken cancellationToken = default);
}