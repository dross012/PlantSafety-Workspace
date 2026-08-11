// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with culture-index-score audit-trail
//                  field alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Culture aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Culture"/> aggregate.</summary>
public sealed class CultureHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Culture aggregate.</summary>
    public CultureId CultureId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Culture index score recorded at the time of this transition.</summary>
    public decimal CultureIndexScoreAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private CultureHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public CultureHistoryEntry(
        Guid historyId, CultureId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal cultureIndexScoreAtTransition = 0m)
    {
        HistoryId                        = historyId;
        CultureId                        = Id;
        PreviousStatus                    = previousStatus;
        NewStatus                         = newStatus;
        CultureIndexScoreAtTransition     = cultureIndexScoreAtTransition;
        ChangedAtUtc                      = changedAtUtc;
        ChangedBy                         = changedBy;
        Notes                             = notes;
    }
}
