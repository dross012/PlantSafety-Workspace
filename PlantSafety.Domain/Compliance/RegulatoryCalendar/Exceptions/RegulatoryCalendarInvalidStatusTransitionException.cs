// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Exceptions;

/// <summary>Thrown when a <see cref="RegulatoryCalendar"/> lifecycle transition is not permitted.</summary>
public sealed class RegulatoryCalendarInvalidStatusTransitionException : RegulatoryCalendarDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RegulatoryCalendarInvalidStatusTransitionException(
        RegulatoryCalendarId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'RegulatoryCalendar' {id} from '{from}' to '{to}'.",
            "RegulatoryCalendar.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
