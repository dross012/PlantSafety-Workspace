// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing TwoHandControls aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Factories;

/// <summary>Default factory implementation for constructing <see cref="TwoHandControls"/> aggregates.</summary>
public sealed class TwoHandControlsFactory : ITwoHandControlsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}