// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 — extended with audit-specific audit-trail fields
//                  (score/findings/rating deltas) alongside the generic status
//                  transition, mirroring the AlertHistoryEntry convention.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition (and,
//                  where relevant, findings/score change) for an Auditing aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Auditing"/> aggregate.</summary>
public sealed class AuditingHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Auditing aggregate.</summary>
    public AuditingId AuditingId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Findings count recorded at the time of this transition.</summary>
    public int FindingsCountAtTransition { get; private set; }

    /// <summary>Compliance score recorded at the time of this transition, if known.</summary>
    public decimal? ScoreAtTransition { get; private set; }

    /// <summary>Compliance rating recorded at the time of this transition.</summary>
    public string? ComplianceRatingAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private AuditingHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public AuditingHistoryEntry(
        Guid historyId, AuditingId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        int findingsCountAtTransition = 0, decimal? scoreAtTransition = null,
        string? complianceRatingAtTransition = null)
    {
        HistoryId                    = historyId;
        AuditingId                   = Id;
        PreviousStatus                = previousStatus;
        NewStatus                     = newStatus;
        FindingsCountAtTransition     = findingsCountAtTransition;
        ScoreAtTransition              = scoreAtTransition;
        ComplianceRatingAtTransition  = complianceRatingAtTransition;
        ChangedAtUtc                  = changedAtUtc;
        ChangedBy                     = changedBy;
        Notes                         = notes;
    }
}
