// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with engagement-score audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for an
//                  Engagement aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Engagement"/> aggregate.</summary>
public sealed class EngagementHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Engagement aggregate.</summary>
    public EngagementId EngagementId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Engagement score recorded at the time of this transition.</summary>
    public decimal EngagementScoreAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private EngagementHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public EngagementHistoryEntry(
        Guid historyId, EngagementId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal engagementScoreAtTransition = 0m)
    {
        HistoryId                       = historyId;
        EngagementId                    = Id;
        PreviousStatus                   = previousStatus;
        NewStatus                        = newStatus;
        EngagementScoreAtTransition      = engagementScoreAtTransition;
        ChangedAtUtc                     = changedAtUtc;
        ChangedBy                        = changedBy;
        Notes                            = notes;
    }
}
