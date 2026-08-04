// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           ILimitExceedanceRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LimitExceedance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces;

/// <summary>Domain repository interface for the <see cref="LimitExceedance"/> aggregate root.</summary>
public interface ILimitExceedanceRepository
{
    /// <summary>Retrieves a LimitExceedance aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance?> GetByIdAsync(LimitExceedanceId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all LimitExceedance aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance>> ListAsync(LimitExceedanceSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LimitExceedance aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing LimitExceedance aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance aggregate, CancellationToken cancellationToken = default);
}