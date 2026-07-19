// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with policy-version/acknowledgment
//                  audit-trail fields alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Policy aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Policy"/> aggregate.</summary>
public sealed class PolicyHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Policy aggregate.</summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Policy version recorded at the time of this transition.</summary>
    public int PolicyVersionAtTransition { get; private set; }

    /// <summary>Acknowledgment count recorded at the time of this transition.</summary>
    public int AcknowledgmentCountAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private PolicyHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public PolicyHistoryEntry(
        Guid historyId, PolicyId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        int policyVersionAtTransition = 1, int acknowledgmentCountAtTransition = 0)
    {
        HistoryId                          = historyId;
        PolicyId                           = Id;
        PreviousStatus                      = previousStatus;
        NewStatus                           = newStatus;
        PolicyVersionAtTransition           = policyVersionAtTransition;
        AcknowledgmentCountAtTransition      = acknowledgmentCountAtTransition;
        ChangedAtUtc                        = changedAtUtc;
        ChangedBy                           = changedBy;
        Notes                               = notes;
    }
}
