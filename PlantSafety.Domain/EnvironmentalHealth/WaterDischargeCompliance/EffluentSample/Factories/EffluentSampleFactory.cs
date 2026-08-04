// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EffluentSample aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Factories;

/// <summary>Default factory implementation for constructing <see cref="EffluentSample"/> aggregates.</summary>
public sealed class EffluentSampleFactory : IEffluentSampleFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}