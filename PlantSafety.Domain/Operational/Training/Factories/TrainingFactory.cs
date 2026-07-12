// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Training aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Factories;

/// <summary>Default factory implementation for constructing <see cref="Training"/> aggregates.</summary>
public sealed class TrainingFactory : ITrainingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Operational.Training.Aggregates.Training Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Operational.Training.Aggregates.Training.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
