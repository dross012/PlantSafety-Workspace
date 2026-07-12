// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           IRegulatoryCalendarFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing RegulatoryCalendar aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;

/// <summary>Factory interface for constructing <see cref="RegulatoryCalendar"/> aggregates.</summary>
public interface IRegulatoryCalendarFactory
{
    /// <summary>Creates a new RegulatoryCalendar aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
