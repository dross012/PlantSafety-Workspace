// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing RegulatoryCalendar aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Factories;

/// <summary>Default factory implementation for constructing <see cref="RegulatoryCalendar"/> aggregates.</summary>
public sealed class RegulatoryCalendarFactory : IRegulatoryCalendarFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
