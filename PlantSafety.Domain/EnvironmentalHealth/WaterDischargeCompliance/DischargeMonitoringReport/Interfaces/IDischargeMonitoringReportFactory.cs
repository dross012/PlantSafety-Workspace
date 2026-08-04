// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           IDischargeMonitoringReportFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing DischargeMonitoringReport aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces;

/// <summary>Factory interface for constructing <see cref="DischargeMonitoringReport"/> aggregates.</summary>
public interface IDischargeMonitoringReportFactory
{
    /// <summary>Creates a new DischargeMonitoringReport aggregate with default field values (base audit fields only).</summary>
    PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}