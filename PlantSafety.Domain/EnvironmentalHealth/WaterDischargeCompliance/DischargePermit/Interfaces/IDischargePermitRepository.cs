// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           IDischargePermitRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DischargePermit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces;

/// <summary>Domain repository interface for the <see cref="DischargePermit"/> aggregate root.</summary>
public interface IDischargePermitRepository
{
    /// <summary>Retrieves a DischargePermit aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit?> GetByIdAsync(DischargePermitId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DischargePermit aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit>> ListAsync(DischargePermitSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DischargePermit aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DischargePermit aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit aggregate, CancellationToken cancellationToken = default);
}