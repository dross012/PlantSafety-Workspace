// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with compliance-status audit-trail field
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  PPEManagement aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="PPEManagement"/> aggregate.</summary>
public sealed class PPEManagementHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent PPEManagement aggregate.</summary>
    public PPEManagementId PPEManagementId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Compliance status recorded at the time of this transition.</summary>
    public string? ComplianceStatusAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private PPEManagementHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public PPEManagementHistoryEntry(
        Guid historyId, PPEManagementId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        string? complianceStatusAtTransition = null)
    {
        HistoryId                     = historyId;
        PPEManagementId                = Id;
        PreviousStatus                  = previousStatus;
        NewStatus                       = newStatus;
        ComplianceStatusAtTransition    = complianceStatusAtTransition;
        ChangedAtUtc                    = changedAtUtc;
        ChangedBy                       = changedBy;
        Notes                           = notes;
    }
}
