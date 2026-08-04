// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a TransportVehicle aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="TransportVehicle"/> aggregate.</summary>
public sealed class TransportVehicleHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent TransportVehicle aggregate.</summary>
    public TransportVehicleId TransportVehicleId { get; private set; }

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
    private TransportVehicleHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public TransportVehicleHistoryEntry(
        Guid historyId, TransportVehicleId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId          = historyId;
        TransportVehicleId = id;
        PreviousStatus      = previousStatus;
        NewStatus           = newStatus;
        ChangedAtUtc         = changedAtUtc;
        ChangedBy            = changedBy;
        Notes                = notes;
    }
}
