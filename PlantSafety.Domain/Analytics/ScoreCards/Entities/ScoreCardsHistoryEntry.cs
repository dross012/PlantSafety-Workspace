// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with performance-index audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  ScoreCards aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="ScoreCards"/> aggregate.</summary>
public sealed class ScoreCardsHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent ScoreCards aggregate.</summary>
    public ScoreCardsId ScoreCardsId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Performance index recorded at the time of this transition.</summary>
    public decimal PerformanceIndexAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ScoreCardsHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ScoreCardsHistoryEntry(
        Guid historyId, ScoreCardsId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal performanceIndexAtTransition = 0m)
    {
        HistoryId                        = historyId;
        ScoreCardsId                     = Id;
        PreviousStatus                    = previousStatus;
        NewStatus                         = newStatus;
        PerformanceIndexAtTransition      = performanceIndexAtTransition;
        ChangedAtUtc                      = changedAtUtc;
        ChangedBy                         = changedBy;
        Notes                             = notes;
    }
}
