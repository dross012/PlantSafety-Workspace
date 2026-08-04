// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing LimitExceedance aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Factories;

/// <summary>Default factory implementation for constructing <see cref="LimitExceedance"/> aggregates.</summary>
public sealed class LimitExceedanceFactory : ILimitExceedanceFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}