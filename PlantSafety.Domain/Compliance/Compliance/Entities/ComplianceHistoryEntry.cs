// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with compliance-score/non-conformance
//                  audit-trail fields alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Compliance aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Compliance"/> aggregate.</summary>
public sealed class ComplianceHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Compliance aggregate.</summary>
    public ComplianceId ComplianceId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Compliance score recorded at the time of this transition.</summary>
    public decimal ComplianceScoreAtTransition { get; private set; }

    /// <summary>Non-conformance count recorded at the time of this transition.</summary>
    public int NonConformanceCountAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ComplianceHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ComplianceHistoryEntry(
        Guid historyId, ComplianceId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        decimal complianceScoreAtTransition = 0m, int nonConformanceCountAtTransition = 0)
    {
        HistoryId                        = historyId;
        ComplianceId                     = Id;
        PreviousStatus                    = previousStatus;
        NewStatus                         = newStatus;
        ComplianceScoreAtTransition       = complianceScoreAtTransition;
        NonConformanceCountAtTransition   = nonConformanceCountAtTransition;
        ChangedAtUtc                      = changedAtUtc;
        ChangedBy                         = changedBy;
        Notes                             = notes;
    }
}
