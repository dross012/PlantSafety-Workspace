// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           IAiSafetyIntelligenceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AiSafetyIntelligence aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Repositories;

/// <summary>Domain repository interface for the <see cref="AiSafetyIntelligence"/> aggregate root.</summary>
public interface IAiSafetyIntelligenceRepository
{
    /// <summary>Retrieves a AiSafetyIntelligence aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<AiSafetyIntelligence?> GetByIdAsync(AiSafetyIntelligenceId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AiSafetyIntelligence aggregate.</summary>
    Task AddAsync(AiSafetyIntelligence aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AiSafetyIntelligence aggregate.</summary>
    Task UpdateAsync(AiSafetyIntelligence aggregate, CancellationToken cancellationToken = default);
}
