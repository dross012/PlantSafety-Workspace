// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single triage-status transition for an
//                  AccessAnomaly aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Entities;

/// <summary>Records a single triage-status transition on an <see cref="AccessAnomaly"/> aggregate.</summary>
public sealed class AccessAnomalyHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent AccessAnomaly aggregate.</summary>
    public AccessAnomalyId AccessAnomalyId { get; private set; }

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
    private AccessAnomalyHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public AccessAnomalyHistoryEntry(
        Guid historyId, AccessAnomalyId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId       = historyId;
        AccessAnomalyId = id;
        PreviousStatus  = previousStatus;
        NewStatus       = newStatus;
        ChangedAtUtc    = changedAtUtc;
        ChangedBy       = changedBy;
        Notes           = notes;
    }
}
