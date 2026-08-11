// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with due-date audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  RegulatoryCalendar aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="RegulatoryCalendar"/> aggregate.</summary>
public sealed class RegulatoryCalendarHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent RegulatoryCalendar aggregate.</summary>
    public RegulatoryCalendarId RegulatoryCalendarId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Due date recorded at the time of this transition, if known.</summary>
    public UtcTimestamp? DueAtUtcAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private RegulatoryCalendarHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public RegulatoryCalendarHistoryEntry(
        Guid historyId, RegulatoryCalendarId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        UtcTimestamp? dueAtUtcAtTransition = null)
    {
        HistoryId               = historyId;
        RegulatoryCalendarId     = Id;
        PreviousStatus           = previousStatus;
        NewStatus                = newStatus;
        DueAtUtcAtTransition     = dueAtUtcAtTransition;
        ChangedAtUtc             = changedAtUtc;
        ChangedBy                = changedBy;
        Notes                    = notes;
    }
}
