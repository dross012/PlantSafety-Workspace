// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPEZoneMapping aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPEZoneMapping"/> aggregates.</summary>
public sealed class PPEZoneMappingFactory : IPPEZoneMappingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}