// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PMScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Factories;

/// <summary>Default factory implementation for constructing <see cref="PMScheduling"/> aggregates.</summary>
public sealed class PMSchedulingFactory : IPMSchedulingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}