// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateItem.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Child entity representing a single inspection line item on a
//                  ChecklistTemplate (e.g. "Guard integrity and secure mounting"
//                  under the Machine Guarding & LOTO Checklists category, per
//                  PlantSafety Program Structure, "Comprehensive Manufacturing
//                  Safety Checklists"). Owned exclusively by the ChecklistTemplate
//                  aggregate root; never referenced directly outside it.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Entities;

/// <summary>A single inspection line item defined on a <see cref="ChecklistTemplate"/>.</summary>
public sealed class ChecklistTemplateItem
{
    /// <summary>Unique identity of this item within its parent template.</summary>
    public Guid ItemId { get; private set; }

    /// <summary>Owning ChecklistTemplate identity.</summary>
    public ChecklistTemplateId ChecklistTemplateId { get; private set; }

    /// <summary>1-based display/execution order of this item within the template.</summary>
    public int Sequence { get; private set; }

    /// <summary>Short inspection instruction text (e.g. "Verification of zero-energy state").</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Whether a "fail" answer on this item constitutes a critical/stop-work condition
    /// (e.g. missing LOTO device, disabled interlock) rather than a routine finding.
    /// </summary>
    public bool IsCritical { get; private set; }

    /// <summary>Optional regulatory citation specific to this item.</summary>
    public string? RegulatoryReference { get; private set; }

    /// <summary>UTC instant this item was added to the template.</summary>
    public UtcTimestamp AddedAtUtc { get; private set; }

    /// <summary>For rehydration only.</summary>
    private ChecklistTemplateItem() { }

    /// <summary>Creates a new checklist template line item.</summary>
    public ChecklistTemplateItem(
        Guid itemId,
        ChecklistTemplateId checklistTemplateId,
        int sequence,
        string description,
        bool isCritical,
        string? regulatoryReference,
        UtcTimestamp addedAtUtc)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException("ItemId must not be empty.", nameof(itemId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Checklist item description must not be empty.", nameof(description));
        if (sequence < 1)
            throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Item sequence must be >= 1.");

        ItemId = itemId;
        ChecklistTemplateId = checklistTemplateId;
        Sequence = sequence;
        Description = description.Trim();
        IsCritical = isCritical;
        RegulatoryReference = regulatoryReference;
        AddedAtUtc = addedAtUtc;
    }
}
