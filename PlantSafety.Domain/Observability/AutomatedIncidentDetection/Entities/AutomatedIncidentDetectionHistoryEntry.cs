// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a AutomatedIncidentDetection aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="AutomatedIncidentDetection"/> aggregate.</summary>
public sealed class AutomatedIncidentDetectionHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent AutomatedIncidentDetection aggregate.</summary>
    public AutomatedIncidentDetectionId AutomatedIncidentDetectionId { get; private set; }

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
    private AutomatedIncidentDetectionHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public AutomatedIncidentDetectionHistoryEntry(
        Guid historyId, AutomatedIncidentDetectionId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        AutomatedIncidentDetectionId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}