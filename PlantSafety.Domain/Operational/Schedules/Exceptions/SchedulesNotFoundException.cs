// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Exceptions;

/// <summary>Thrown when a <see cref="Schedules"/> aggregate lookup finds no match.</summary>
public sealed class SchedulesNotFoundException : SchedulesDomainException
{
    /// <summary>Creates a not-found exception for the given Schedules identity.</summary>
    public SchedulesNotFoundException(SchedulesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Schedules with id '{id}' was not found.",
            "Schedules.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
