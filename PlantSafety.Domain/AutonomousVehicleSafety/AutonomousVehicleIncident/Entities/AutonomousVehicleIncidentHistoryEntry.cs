// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for an
//                  AutonomousVehicleIncident aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Entities;

/// <summary>Records a single lifecycle status transition on an <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate.</summary>
public sealed class AutonomousVehicleIncidentHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent AutonomousVehicleIncident aggregate.</summary>
    public AutonomousVehicleIncidentId AutonomousVehicleIncidentId { get; private set; }

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
    private AutonomousVehicleIncidentHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public AutonomousVehicleIncidentHistoryEntry(
        Guid historyId, AutonomousVehicleIncidentId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId = historyId;
        AutonomousVehicleIncidentId = id;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedAtUtc = changedAtUtc;
        ChangedBy = changedBy;
        Notes = notes;
    }
}
