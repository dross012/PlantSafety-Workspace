// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing DischargeMonitoringReport aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Factories;

/// <summary>Default factory implementation for constructing <see cref="DischargeMonitoringReport"/> aggregates.</summary>
public sealed class DischargeMonitoringReportFactory : IDischargeMonitoringReportFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}