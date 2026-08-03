// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementTimelineEntry.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity recording a single chronological timeline entry for an
//                  IncidentManagement aggregate (status change, note, escalation, resolution).
//                  Appended by the aggregate's lifecycle methods (Create/AssignTechnician/Escalate/
//                  AddNote/Resolve/Archive) and persisted by the Infrastructure repository to
//                  [SafetyEvents].[IncidentTimelineEntry] for the read-side
//                  GetIncidentTimelineQuery (mirrors the EmergencyManagementHistoryEntry /
//                  ObservationHistoryEntry convention used by sibling SafetyEvents aggregates).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Entities;

/// <summary>Records a single timeline entry for an <see cref="Aggregates.IncidentManagement"/> aggregate.</summary>
public sealed class IncidentManagementTimelineEntry
{
    /// <summary>Unique identity of this timeline entry.</summary>
    public Guid EntryId { get; private set; }

    /// <summary>Identity of the parent IncidentManagement aggregate.</summary>
    public IncidentManagementId IncidentManagementId { get; private set; }

    /// <summary>UTC instant this entry occurred.</summary>
    public UtcTimestamp TimestampUtc { get; private set; }

    /// <summary>Actor who performed/generated this entry (rendered display form).</summary>
    public string Actor { get; private set; }

    /// <summary>Event type: StatusChange, OperatorAction, Note, Escalation, or Resolution.</summary>
    public string EventType { get; private set; }

    /// <summary>Severity associated with this event at the time it occurred.</summary>
    public string Severity { get; private set; }

    /// <summary>Human-readable description of the event.</summary>
    public string Description { get; private set; }

    /// <summary>For ORM/read-model rehydration only.</summary>
    private IncidentManagementTimelineEntry() { }

    /// <summary>Creates a new timeline entry.</summary>
    public IncidentManagementTimelineEntry(
        Guid entryId,
        IncidentManagementId incidentManagementId,
        UtcTimestamp timestampUtc,
        string actor,
        string eventType,
        string severity,
        string description)
    {
        EntryId = entryId;
        IncidentManagementId = incidentManagementId;
        TimestampUtc = timestampUtc;
        Actor = actor;
        EventType = eventType;
        Severity = severity;
        Description = description;
    }
}
