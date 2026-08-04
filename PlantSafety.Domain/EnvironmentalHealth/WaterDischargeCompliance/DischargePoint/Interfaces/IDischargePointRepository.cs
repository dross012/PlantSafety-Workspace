// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           IDischargePointRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DischargePoint aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces;

/// <summary>Domain repository interface for the <see cref="DischargePoint"/> aggregate root.</summary>
public interface IDischargePointRepository
{
    /// <summary>Retrieves a DischargePoint aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint?> GetByIdAsync(DischargePointId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DischargePoint aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint>> ListAsync(DischargePointSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DischargePoint aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DischargePoint aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint aggregate, CancellationToken cancellationToken = default);
}