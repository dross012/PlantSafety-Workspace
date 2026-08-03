// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsHistoryEntry.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Append-only audit-trail child entity attached to a GlobalSafetyInsightsAggregate
//                  snapshot: one entry per computation run plus one entry per received
//                  CrossSiteActionEvent (Phase 8 §4 event-mesh extension -- see
//                  CrossSiteActionConsumer, which records the not-yet-built Multi-Site Coordination
//                  workflow's actions here since no coordination command exists yet to attach them
//                  to a purpose-built aggregate). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Entities;

/// <summary>A single append-only audit-trail entry attached to a GlobalSafetyInsightsAggregate.</summary>
public sealed class GlobalSafetyInsightsHistoryEntry
{
    /// <summary>Owning aggregate identity.</summary>
    public Guid GlobalSafetyInsightsId { get; }

    /// <summary>Short discriminator for the kind of event recorded (e.g. "InsightComputed", "CrossSiteAction").</summary>
    public string EntryType { get; }

    /// <summary>Human-readable description of what happened.</summary>
    public string Description { get; }

    /// <summary>UTC instant the entry was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; }

    /// <summary>Identity of the actor responsible for this entry, if a user.</summary>
    public Guid? ActorUserId { get; }

    /// <summary>Creates a new audit-trail entry.</summary>
    public GlobalSafetyInsightsHistoryEntry(
        Guid globalSafetyInsightsId, string entryType, string description, UtcTimestamp recordedAtUtc, Guid? actorUserId)
    {
        if (string.IsNullOrWhiteSpace(entryType))
            throw new ArgumentException("EntryType must not be empty.", nameof(entryType));

        GlobalSafetyInsightsId = globalSafetyInsightsId;
        EntryType = entryType.Trim();
        Description = (description ?? string.Empty).Trim();
        RecordedAtUtc = recordedAtUtc;
        ActorUserId = actorUserId;
    }

    // For rehydration only.
    private GlobalSafetyInsightsHistoryEntry()
    {
        EntryType = string.Empty;
        Description = string.Empty;
    }
}
