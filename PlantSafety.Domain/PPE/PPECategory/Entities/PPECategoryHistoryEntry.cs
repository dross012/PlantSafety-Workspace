// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a PPECategory aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="PPECategory"/> aggregate.</summary>
public sealed class PPECategoryHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent PPECategory aggregate.</summary>
    public PPECategoryId PPECategoryId { get; private set; }

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
    private PPECategoryHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public PPECategoryHistoryEntry(
        Guid historyId, PPECategoryId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        PPECategoryId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}