// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionItemResult.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Child entity recording the pass/fail result of a single
//                  ChecklistTemplateItem as evaluated during a ChecklistCompletion
//                  execution. Owned exclusively by the ChecklistCompletion
//                  aggregate root; never referenced directly outside it.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Entities;

/// <summary>Result of a single template item as evaluated during a <see cref="ChecklistCompletion"/>.</summary>
public sealed class ChecklistCompletionItemResult
{
    /// <summary>Unique identity of this result record.</summary>
    public Guid ItemResultId { get; private set; }

    /// <summary>Owning ChecklistCompletion identity.</summary>
    public ChecklistCompletionId ChecklistCompletionId { get; private set; }

    /// <summary>Identity of the ChecklistTemplateItem this result answers.</summary>
    public Guid ChecklistTemplateItemId { get; private set; }

    /// <summary>Whether the item passed inspection.</summary>
    public bool Passed { get; private set; }

    /// <summary>Whether this failed item was flagged as a critical/stop-work condition.</summary>
    public bool IsCriticalFailure { get; private set; }

    /// <summary>Optional inspector notes/findings for this item.</summary>
    public string? Notes { get; private set; }

    /// <summary>UTC instant this result was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>For rehydration only.</summary>
    private ChecklistCompletionItemResult() { }

    /// <summary>Creates a new item result record.</summary>
    public ChecklistCompletionItemResult(
        Guid itemResultId,
        ChecklistCompletionId checklistCompletionId,
        Guid checklistTemplateItemId,
        bool passed,
        bool isCriticalFailure,
        string? notes,
        UtcTimestamp recordedAtUtc)
    {
        if (itemResultId == Guid.Empty)
            throw new ArgumentException("ItemResultId must not be empty.", nameof(itemResultId));
        if (checklistTemplateItemId == Guid.Empty)
            throw new ArgumentException("ChecklistTemplateItemId must not be empty.", nameof(checklistTemplateItemId));

        ItemResultId = itemResultId;
        ChecklistCompletionId = checklistCompletionId;
        ChecklistTemplateItemId = checklistTemplateItemId;
        Passed = passed;
        IsCriticalFailure = isCriticalFailure;
        Notes = notes;
        RecordedAtUtc = recordedAtUtc;
    }
}
