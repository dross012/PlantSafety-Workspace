// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with certification-specific audit-trail
//                  fields (expiry snapshot) alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for a
//                  Certification aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="Certification"/> aggregate.</summary>
public sealed class CertificationHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent Certification aggregate.</summary>
    public CertificationId CertificationId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Expiry timestamp recorded at the time of this transition, if known.</summary>
    public UtcTimestamp? ExpiresAtUtcAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private CertificationHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public CertificationHistoryEntry(
        Guid historyId, CertificationId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        UtcTimestamp? expiresAtUtcAtTransition = null)
    {
        HistoryId                 = historyId;
        CertificationId            = Id;
        PreviousStatus             = previousStatus;
        NewStatus                  = newStatus;
        ExpiresAtUtcAtTransition   = expiresAtUtcAtTransition;
        ChangedAtUtc               = changedAtUtc;
        ChangedBy                  = changedBy;
        Notes                      = notes;
    }
}
