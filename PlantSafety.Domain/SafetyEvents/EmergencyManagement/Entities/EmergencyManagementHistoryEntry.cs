// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementHistoryEntry.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended with severity/casualty audit-trail fields
//                  alongside the generic status transition.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition for an
//                  EmergencyManagement aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="EmergencyManagement"/> aggregate.</summary>
public sealed class EmergencyManagementHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent EmergencyManagement aggregate.</summary>
    public EmergencyManagementId EmergencyManagementId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>Severity recorded at the time of this transition.</summary>
    public string? SeverityAtTransition { get; private set; }

    /// <summary>Casualty count recorded at the time of this transition.</summary>
    public int CasualtyCountAtTransition { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private EmergencyManagementHistoryEntry() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        ChangedBy = null!;
        NewStatus = null!;
        PreviousStatus = null!;
    }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public EmergencyManagementHistoryEntry(
        Guid historyId, EmergencyManagementId Id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null,
        string? severityAtTransition = null, int casualtyCountAtTransition = 0)
    {
        HistoryId                    = historyId;
        EmergencyManagementId        = Id;
        PreviousStatus                = previousStatus;
        NewStatus                     = newStatus;
        SeverityAtTransition          = severityAtTransition;
        CasualtyCountAtTransition     = casualtyCountAtTransition;
        ChangedAtUtc                  = changedAtUtc;
        ChangedBy                     = changedBy;
        Notes                         = notes;
    }
}
