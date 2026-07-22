// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PreQualificationVetting aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Factories;

/// <summary>Default factory implementation for constructing <see cref="PreQualificationVetting"/> aggregates.</summary>
public sealed class PreQualificationVettingFactory : IPreQualificationVettingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}