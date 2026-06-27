// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Schedules aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Factories;

/// <summary>Default factory implementation for constructing <see cref="Schedules"/> aggregates.</summary>
public sealed class SchedulesFactory : ISchedulesFactory
{
    /// <inheritdoc/>
    public Schedules Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Schedules.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
