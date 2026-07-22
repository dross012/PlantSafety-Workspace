// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateHistoryEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single lifecycle transition
//                  for a ChecklistTemplate aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Entities;

/// <summary>Records a single lifecycle status transition on a <see cref="ChecklistTemplate"/> aggregate.</summary>
public sealed class ChecklistTemplateHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent ChecklistTemplate aggregate.</summary>
    public ChecklistTemplateId ChecklistTemplateId { get; private set; }

    /// <summary>Status before the transition.</summary>
    public string PreviousStatus { get; private set; }

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; private set; }

    /// <summary>UTC instant of the transition.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who performed the transition.</summary>
    public string ChangedBy { get; private set; }

    /// <summary>Optional notes recorded at the time of the transition.</summary>
    public string? Notes { get; private set; }

    /// <summary>For rehydration only.</summary>
    private ChecklistTemplateHistoryEntry() { }

    /// <summary>Creates a new history entry for the given transition.</summary>
    public ChecklistTemplateHistoryEntry(
        Guid historyId, ChecklistTemplateId id,
        string previousStatus, string newStatus,
        UtcTimestamp changedAtUtc, string changedBy, string? notes = null)
    {
        HistoryId      = historyId;
        ChecklistTemplateId       = id;
        PreviousStatus = previousStatus;
        NewStatus      = newStatus;
        ChangedAtUtc   = changedAtUtc;
        ChangedBy      = changedBy;
        Notes          = notes;
    }
}