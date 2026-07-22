// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EnvironmentalAssetMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Factories;

/// <summary>Default factory implementation for constructing <see cref="EnvironmentalAssetMonitoring"/> aggregates.</summary>
public sealed class EnvironmentalAssetMonitoringFactory : IEnvironmentalAssetMonitoringFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}