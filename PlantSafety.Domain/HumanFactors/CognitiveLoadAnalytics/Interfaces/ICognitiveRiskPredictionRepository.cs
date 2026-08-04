// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ICognitiveRiskPredictionRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the CognitiveRiskPrediction aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.CognitiveRiskPrediction"/> aggregate root.</summary>
public interface ICognitiveRiskPredictionRepository
{
    /// <summary>Retrieves a CognitiveRiskPrediction aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.CognitiveRiskPrediction?> GetByIdAsync(CognitiveRiskPredictionId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new CognitiveRiskPrediction aggregate.</summary>
    Task AddAsync(Aggregates.CognitiveRiskPrediction aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CognitiveRiskPrediction aggregate.</summary>
    Task UpdateAsync(Aggregates.CognitiveRiskPrediction aggregate, CancellationToken cancellationToken = default);
}
