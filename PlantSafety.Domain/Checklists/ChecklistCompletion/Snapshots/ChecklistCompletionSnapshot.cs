// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ChecklistCompletion aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ChecklistCompletion"/> aggregate for fast rehydration.</summary>
public sealed record ChecklistCompletionSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ChecklistCompletionId ChecklistCompletionId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ChecklistCompletionStatus Status { get; init; }

    /// <summary>Identity of the ChecklistTemplate this completion was performed against.</summary>
    public Guid ChecklistTemplateId { get; init; }

    /// <summary>Overall outcome classification.</summary>
    public string Outcome { get; init; } = "Pending";

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}