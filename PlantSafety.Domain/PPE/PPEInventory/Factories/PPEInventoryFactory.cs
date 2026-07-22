// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPEInventory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPEInventory"/> aggregates.</summary>
public sealed class PPEInventoryFactory : IPPEInventoryFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}