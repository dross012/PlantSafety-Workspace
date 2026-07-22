// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ChecklistTemplate aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ChecklistTemplate"/> aggregate for fast rehydration.</summary>
public sealed record ChecklistTemplateSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ChecklistTemplateId ChecklistTemplateId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ChecklistTemplateStatus Status { get; init; }

    /// <summary>Human-readable checklist template title.</summary>
    public string TemplateTitle { get; init; } = string.Empty;

    /// <summary>Checklist category code.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}