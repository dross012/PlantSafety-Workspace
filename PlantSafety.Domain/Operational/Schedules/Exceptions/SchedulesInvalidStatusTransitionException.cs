// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Exceptions;

/// <summary>Thrown when a <see cref="Schedules"/> lifecycle transition is not permitted.</summary>
public sealed class SchedulesInvalidStatusTransitionException : SchedulesDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SchedulesInvalidStatusTransitionException(
        SchedulesId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'Schedules' {id} from '{from}' to '{to}'.",
            "Schedules.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
