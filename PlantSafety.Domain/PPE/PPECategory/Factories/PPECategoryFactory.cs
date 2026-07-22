// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPECategory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPECategory"/> aggregates.</summary>
public sealed class PPECategoryFactory : IPPECategoryFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}