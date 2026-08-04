// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for an
//                  AutonomousVehicleAsset aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Entities;

/// <summary>Records a single lifecycle status transition on an <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate.</summary>
public sealed class AutonomousVehicleAssetHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent AutonomousVehicleAsset aggregate.</summary>
    public AutonomousVehicleAssetId AutonomousVehicleAssetId { get; private set; }

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
    private AutonomousVehicleAssetHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public AutonomousVehicleAssetHistoryEntry(
        Guid historyId, AutonomousVehicleAssetId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId = historyId;
        AutonomousVehicleAssetId = id;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedAtUtc = changedAtUtc;
        ChangedBy = changedBy;
        Notes = notes;
    }
}
