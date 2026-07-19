// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with risk-score audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  RiskManagement aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="RiskManagement"/> aggregate.</summary>
public sealed class RiskManagementHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent RiskManagement aggregate.</summary>
    public RiskManagementId RiskManagementId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Computed risk score (likelihood * severity) recorded at the time of this transition.</summary>
    public int RiskScoreAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private RiskManagementHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public RiskManagementHistoryEntry(
        Guid historyId, RiskManagementId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        int riskScoreAtTransition = 0)
    {
        HistoryId                  = historyId;
        RiskManagementId            = Id;
        PreviousStatus               = previousStatus;
        NewStatus                    = newStatus;
        RiskScoreAtTransition        = riskScoreAtTransition;
        ChangedAtUtc                 = changedAtUtc;
        ChangedBy                    = changedBy;
        Notes                        = notes;
    }
}
