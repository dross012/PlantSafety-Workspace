// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing StopWorkEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Factories;

/// <summary>Default factory implementation for constructing <see cref="StopWorkEvent"/> aggregates.</summary>
public sealed class StopWorkEventFactory : IStopWorkEventFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}