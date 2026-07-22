// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a ErgonomicsAndMaterialHandling aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="ErgonomicsAndMaterialHandling"/> aggregate.</summary>
public sealed class ErgonomicsAndMaterialHandlingHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent ErgonomicsAndMaterialHandling aggregate.</summary>
    public ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId { get; private set; }

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
    private ErgonomicsAndMaterialHandlingHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ErgonomicsAndMaterialHandlingHistoryEntry(
        Guid historyId, ErgonomicsAndMaterialHandlingId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        ErgonomicsAndMaterialHandlingId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}