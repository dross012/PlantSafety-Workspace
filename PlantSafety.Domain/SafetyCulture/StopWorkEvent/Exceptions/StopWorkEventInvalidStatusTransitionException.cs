// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a StopWorkEvent lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Exceptions;

/// <summary>Thrown when a <see cref="StopWorkEvent"/> lifecycle transition is not permitted.</summary>
public sealed class StopWorkEventInvalidStatusTransitionException : StopWorkEventDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public StopWorkEventInvalidStatusTransitionException(
        StopWorkEventId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'StopWorkEvent' " + id + " from '" + from + "' to '" + to + "'.",
            "StopWorkEvent.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}