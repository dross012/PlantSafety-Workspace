// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazardIsolationSequencing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Factories;

/// <summary>Default factory implementation for constructing <see cref="HazardIsolationSequencing"/> aggregates.</summary>
public sealed class HazardIsolationSequencingFactory : IHazardIsolationSequencingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}