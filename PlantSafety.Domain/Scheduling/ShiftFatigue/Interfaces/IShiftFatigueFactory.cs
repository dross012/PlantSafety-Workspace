// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           IShiftFatigueFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ShiftFatigue aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces;

/// <summary>Factory interface for constructing <see cref="ShiftFatigue"/> aggregates.</summary>
public interface IShiftFatigueFactory
{
    /// <summary>Creates a new ShiftFatigue aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}