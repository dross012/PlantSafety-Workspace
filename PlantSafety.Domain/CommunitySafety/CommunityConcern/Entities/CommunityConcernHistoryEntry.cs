// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity recording a single chronological timeline entry for a
//                  CommunityConcern aggregate (status change, escalation, resolution). Appended by
//                  the aggregate's lifecycle methods and persisted by the Infrastructure repository
//                  for read-side timeline queries. Mirrors the IncidentManagementTimelineEntry /
//                  EmergencyManagementHistoryEntry convention used by sibling modules. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Entities;

/// <summary>Records a single timeline entry for a <see cref="Aggregates.CommunityConcern"/> aggregate.</summary>
public sealed class CommunityConcernHistoryEntry
{
    /// <summary>Unique identity of this history entry.</summary>
    public Guid EntryId { get; private set; }

    /// <summary>Identity of the parent CommunityConcern aggregate.</summary>
    public CommunityConcernId CommunityConcernId { get; private set; }

    /// <summary>UTC instant this entry occurred.</summary>
    public UtcTimestamp TimestampUtc { get; private set; }

    /// <summary>Actor who performed/generated this entry (rendered display form).</summary>
    public string Actor { get; private set; }

    /// <summary>Event type: Logged, Escalated, Resolved, Archived.</summary>
    public string EventType { get; private set; }

    /// <summary>Human-readable description of the event.</summary>
    public string Description { get; private set; }

    /// <summary>For ORM/read-model rehydration only.</summary>
    private CommunityConcernHistoryEntry() { }

    /// <summary>Creates a new history entry.</summary>
    public CommunityConcernHistoryEntry(
        Guid entryId,
        CommunityConcernId communityConcernId,
        UtcTimestamp timestampUtc,
        string actor,
        string eventType,
        string description)
    {
        EntryId = entryId;
        CommunityConcernId = communityConcernId;
        TimestampUtc = timestampUtc;
        Actor = actor;
        EventType = eventType;
        Description = description;
    }
}
