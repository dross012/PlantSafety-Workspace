// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for ChecklistTemplate domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ChecklistTemplate domain events.</summary>
public readonly record struct ChecklistTemplateEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistTemplateEventId wrapping a non-empty GUID.</summary>
    public ChecklistTemplateEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistTemplateEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistTemplateEventId using Uuid7.</summary>
    public static ChecklistTemplateEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}