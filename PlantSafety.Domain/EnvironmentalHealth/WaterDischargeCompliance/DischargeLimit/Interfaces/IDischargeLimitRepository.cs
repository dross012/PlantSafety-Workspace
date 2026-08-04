// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           IDischargeLimitRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DischargeLimit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces;

/// <summary>Domain repository interface for the <see cref="DischargeLimit"/> aggregate root.</summary>
public interface IDischargeLimitRepository
{
    /// <summary>Retrieves a DischargeLimit aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit?> GetByIdAsync(DischargeLimitId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DischargeLimit aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit>> ListAsync(DischargeLimitSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DischargeLimit aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DischargeLimit aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit aggregate, CancellationToken cancellationToken = default);
}