// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPETrainingFit aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPETrainingFit"/> aggregates.</summary>
public sealed class PPETrainingFitFactory : IPPETrainingFitFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}