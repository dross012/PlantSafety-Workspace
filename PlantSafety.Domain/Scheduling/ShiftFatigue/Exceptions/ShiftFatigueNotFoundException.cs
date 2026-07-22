// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ShiftFatigue aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Exceptions;

/// <summary>Thrown when a <see cref="ShiftFatigue"/> aggregate lookup finds no match.</summary>
public sealed class ShiftFatigueNotFoundException : ShiftFatigueDomainException
{
    /// <summary>Creates a not-found exception for the given ShiftFatigue identity.</summary>
    public ShiftFatigueNotFoundException(ShiftFatigueId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"ShiftFatigue with id '{id}' was not found.",
            "ShiftFatigue.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}