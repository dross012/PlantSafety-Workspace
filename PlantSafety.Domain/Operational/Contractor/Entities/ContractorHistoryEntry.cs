// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with readiness-score/qualification
//                  audit-trail fields alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Contractor aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Contractor"/> aggregate.</summary>
public sealed class ContractorHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Contractor aggregate.</summary>
    public ContractorId ContractorId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Readiness score recorded at the time of this transition.</summary>
    public decimal ReadinessScoreAtTransition { get; private set; }

    /// <summary>Qualification status recorded at the time of this transition.</summary>
    public string? QualificationStatusAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ContractorHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ContractorHistoryEntry(
        Guid historyId, ContractorId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal readinessScoreAtTransition = 0m, string? qualificationStatusAtTransition = null)
    {
        HistoryId                          = historyId;
        ContractorId                       = Id;
        PreviousStatus                      = previousStatus;
        NewStatus                           = newStatus;
        ReadinessScoreAtTransition          = readinessScoreAtTransition;
        QualificationStatusAtTransition     = qualificationStatusAtTransition;
        ChangedAtUtc                        = changedAtUtc;
        ChangedBy                           = changedBy;
        Notes                               = notes;
    }
}
