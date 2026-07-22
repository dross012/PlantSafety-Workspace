// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a PPEBehavioralMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="PPEBehavioralMonitoring"/> aggregate.</summary>
public sealed class PPEBehavioralMonitoringHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent PPEBehavioralMonitoring aggregate.</summary>
    public PPEBehavioralMonitoringId PPEBehavioralMonitoringId { get; private set; }

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
    private PPEBehavioralMonitoringHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public PPEBehavioralMonitoringHistoryEntry(
        Guid historyId, PPEBehavioralMonitoringId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        PPEBehavioralMonitoringId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}