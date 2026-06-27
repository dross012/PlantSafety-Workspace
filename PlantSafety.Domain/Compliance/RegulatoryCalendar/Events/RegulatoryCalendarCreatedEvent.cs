// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Events;

/// <summary>Raised when a new <see cref="RegulatoryCalendar"/> aggregate is created.</summary>
public sealed record RegulatoryCalendarCreatedEvent(
    RegulatoryCalendarEventId EventId,
    RegulatoryCalendarId      RegulatoryCalendarId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IRegulatoryCalendarDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RegulatoryCalendarCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
