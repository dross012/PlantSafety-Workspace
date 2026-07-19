// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with revision-number audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Procedures aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Procedures"/> aggregate.</summary>
public sealed class ProceduresHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Procedures aggregate.</summary>
    public ProceduresId ProceduresId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Revision number recorded at the time of this transition.</summary>
    public int RevisionNumberAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ProceduresHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ProceduresHistoryEntry(
        Guid historyId, ProceduresId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        int revisionNumberAtTransition = 1)
    {
        HistoryId                    = historyId;
        ProceduresId                 = Id;
        PreviousStatus                = previousStatus;
        NewStatus                     = newStatus;
        RevisionNumberAtTransition    = revisionNumberAtTransition;
        ChangedAtUtc                  = changedAtUtc;
        ChangedBy                     = changedBy;
        Notes                         = notes;
    }
}
