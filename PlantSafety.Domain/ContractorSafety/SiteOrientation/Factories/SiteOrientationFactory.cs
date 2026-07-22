// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SiteOrientation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Factories;

/// <summary>Default factory implementation for constructing <see cref="SiteOrientation"/> aggregates.</summary>
public sealed class SiteOrientationFactory : ISiteOrientationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}