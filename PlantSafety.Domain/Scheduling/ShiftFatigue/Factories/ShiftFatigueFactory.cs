// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ShiftFatigue aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Factories;

/// <summary>Default factory implementation for constructing <see cref="ShiftFatigue"/> aggregates.</summary>
public sealed class ShiftFatigueFactory : IShiftFatigueFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}