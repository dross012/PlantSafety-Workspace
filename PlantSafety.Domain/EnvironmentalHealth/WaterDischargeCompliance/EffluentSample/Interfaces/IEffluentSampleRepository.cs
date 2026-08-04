// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           IEffluentSampleRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EffluentSample aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces;

/// <summary>Domain repository interface for the <see cref="EffluentSample"/> aggregate root.</summary>
public interface IEffluentSampleRepository
{
    /// <summary>Retrieves a EffluentSample aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample?> GetByIdAsync(EffluentSampleId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EffluentSample aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample>> ListAsync(EffluentSampleSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EffluentSample aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EffluentSample aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample aggregate, CancellationToken cancellationToken = default);
}