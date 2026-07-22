// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing LotoSystems aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Factories;

/// <summary>Default factory implementation for constructing <see cref="LotoSystems"/> aggregates.</summary>
public sealed class LotoSystemsFactory : ILotoSystemsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}