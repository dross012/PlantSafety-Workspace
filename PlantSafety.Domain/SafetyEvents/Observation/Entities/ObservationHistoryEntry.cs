// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with corrective-action audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for an
//                  Observation aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Observation"/> aggregate.</summary>
public sealed class ObservationHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Observation aggregate.</summary>
    public ObservationId ObservationId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Corrective-action-required flag recorded at the time of this transition.</summary>
    public bool CorrectiveActionRequiredAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ObservationHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ObservationHistoryEntry(
        Guid historyId, ObservationId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        bool correctiveActionRequiredAtTransition = false)
    {
        HistoryId                                 = historyId;
        ObservationId                              = Id;
        PreviousStatus                              = previousStatus;
        NewStatus                                   = newStatus;
        CorrectiveActionRequiredAtTransition        = correctiveActionRequiredAtTransition;
        ChangedAtUtc                                = changedAtUtc;
        ChangedBy                                   = changedBy;
        Notes                                       = notes;
    }
}
