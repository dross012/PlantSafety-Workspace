// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           IAiGovernanceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AiGovernance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Repositories;

/// <summary>Domain repository interface for the <see cref="AiGovernance"/> aggregate root.</summary>
public interface IAiGovernanceRepository
{
    /// <summary>Retrieves a AiGovernance aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance?> GetByIdAsync(AiGovernanceId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AiGovernance aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AiGovernance aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, CancellationToken cancellationToken = default);
}
