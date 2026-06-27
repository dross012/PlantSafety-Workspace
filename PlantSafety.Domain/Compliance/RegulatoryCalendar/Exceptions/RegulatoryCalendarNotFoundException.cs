// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Exceptions;

/// <summary>Thrown when a <see cref="RegulatoryCalendar"/> aggregate lookup finds no match.</summary>
public sealed class RegulatoryCalendarNotFoundException : RegulatoryCalendarDomainException
{
    /// <summary>Creates a not-found exception for the given RegulatoryCalendar identity.</summary>
    public RegulatoryCalendarNotFoundException(RegulatoryCalendarId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "RegulatoryCalendar with id '{id}' was not found.",
            "RegulatoryCalendar.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
