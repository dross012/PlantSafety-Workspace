// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EnvironmentalHealthMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Factories;

/// <summary>Default factory implementation for constructing <see cref="EnvironmentalHealthMonitoring"/> aggregates.</summary>
public sealed class EnvironmentalHealthMonitoringFactory : IEnvironmentalHealthMonitoringFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}