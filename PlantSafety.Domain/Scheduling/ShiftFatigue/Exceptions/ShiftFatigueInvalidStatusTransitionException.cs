// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ShiftFatigue lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Exceptions;

/// <summary>Thrown when a <see cref="ShiftFatigue"/> lifecycle transition is not permitted.</summary>
public sealed class ShiftFatigueInvalidStatusTransitionException : ShiftFatigueDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ShiftFatigueInvalidStatusTransitionException(
        ShiftFatigueId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'ShiftFatigue' {id} from '{from}' to '{to}'.",
            "ShiftFatigue.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}