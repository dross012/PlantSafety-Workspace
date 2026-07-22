// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a Dashboard
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Aggregates.Dashboard"/> aggregate.</summary>
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

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private DashboardHistoryEntry() { PreviousStatus = string.Empty; NewStatus = string.Empty; ChangedBy = string.Empty; }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public DashboardHistoryEntry(
        Guid historyId, DashboardId dashboardId,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        DashboardId    = dashboardId;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}
