// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with published-state audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Dashboard aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Dashboard"/> aggregate.</summary>
public sealed class DashboardHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Dashboard aggregate.</summary>
    public DashboardId DashboardId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Published-state flag recorded at the time of this transition.</summary>
    public bool IsPublishedAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private DashboardHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public DashboardHistoryEntry(
        Guid historyId, DashboardId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        bool isPublishedAtTransition = false)
    {
        HistoryId                    = historyId;
        DashboardId                  = Id;
        PreviousStatus                = previousStatus;
        NewStatus                     = newStatus;
        IsPublishedAtTransition       = isPublishedAtTransition;
        ChangedAtUtc                  = changedAtUtc;
        ChangedBy                     = changedBy;
        Notes                         = notes;
    }
}
