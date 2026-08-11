// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with completion/exception audit-trail
//                  fields alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Checklists aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Checklists"/> aggregate.</summary>
public sealed class ChecklistsHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Checklists aggregate.</summary>
    public ChecklistsId ChecklistsId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Completed items count recorded at the time of this transition.</summary>
    public int CompletedItemsAtTransition { get; private set; }

    /// <summary>Exception count recorded at the time of this transition.</summary>
    public int ExceptionCountAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ChecklistsHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ChecklistsHistoryEntry(
        Guid historyId, ChecklistsId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        int completedItemsAtTransition = 0, int exceptionCountAtTransition = 0)
    {
        HistoryId                     = historyId;
        ChecklistsId                  = Id;
        PreviousStatus                 = previousStatus;
        NewStatus                      = newStatus;
        CompletedItemsAtTransition     = completedItemsAtTransition;
        ExceptionCountAtTransition     = exceptionCountAtTransition;
        ChangedAtUtc                   = changedAtUtc;
        ChangedBy                      = changedBy;
        Notes                          = notes;
    }
}
