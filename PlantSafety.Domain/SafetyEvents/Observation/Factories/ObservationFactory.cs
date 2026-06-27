// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Observation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Factories;

/// <summary>Default factory implementation for constructing <see cref="Observation"/> aggregates.</summary>
public sealed class ObservationFactory : IObservationFactory
{
    /// <inheritdoc/>
    public Observation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Observation.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
