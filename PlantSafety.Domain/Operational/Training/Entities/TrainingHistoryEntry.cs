// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with score audit-trail field alongside
//                  the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Training aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Training"/> aggregate.</summary>
public sealed class TrainingHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Training aggregate.</summary>
    public TrainingId TrainingId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Assessment score recorded at the time of this transition, if known.</summary>
    public decimal? ScoreAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private TrainingHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public TrainingHistoryEntry(
        Guid historyId, TrainingId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal? scoreAtTransition = null)
    {
        HistoryId             = historyId;
        TrainingId            = Id;
        PreviousStatus         = previousStatus;
        NewStatus              = newStatus;
        ScoreAtTransition      = scoreAtTransition;
        ChangedAtUtc           = changedAtUtc;
        ChangedBy              = changedBy;
        Notes                  = notes;
    }
}
