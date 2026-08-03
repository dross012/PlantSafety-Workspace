// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementNoteAddedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a technician/operator note is appended to an IncidentManagement
//                  aggregate's timeline via AddNote. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when a note is added to an <see cref="Aggregates.IncidentManagement"/> aggregate.</summary>
public sealed record IncidentManagementNoteAddedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    Guid                      NoteId,
    string                    Text,
    UtcTimestamp              AddedAtUtc,
    PerformedByActor          Actor,
    CorrelationId             CorrelationId,
    CausationId               CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementNoteAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AddedAtUtc;
}
