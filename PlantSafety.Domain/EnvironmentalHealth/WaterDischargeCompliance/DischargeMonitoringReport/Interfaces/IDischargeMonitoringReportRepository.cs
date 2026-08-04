// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           IDischargeMonitoringReportRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DischargeMonitoringReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces;

/// <summary>Domain repository interface for the <see cref="DischargeMonitoringReport"/> aggregate root.</summary>
public interface IDischargeMonitoringReportRepository
{
    /// <summary>Retrieves a DischargeMonitoringReport aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport?> GetByIdAsync(DischargeMonitoringReportId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DischargeMonitoringReport aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport>> ListAsync(DischargeMonitoringReportSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DischargeMonitoringReport aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DischargeMonitoringReport aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport aggregate, CancellationToken cancellationToken = default);
}