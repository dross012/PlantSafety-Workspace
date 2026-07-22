// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a RoutineSafetyAuditsIncidentReporting aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate.</summary>
public sealed class RoutineSafetyAuditsIncidentReportingHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent RoutineSafetyAuditsIncidentReporting aggregate.</summary>
    public RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId { get; private set; }

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
    private RoutineSafetyAuditsIncidentReportingHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public RoutineSafetyAuditsIncidentReportingHistoryEntry(
        Guid historyId, RoutineSafetyAuditsIncidentReportingId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        RoutineSafetyAuditsIncidentReportingId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}