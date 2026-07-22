// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ChecklistTemplate aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ChecklistTemplate aggregate root.</summary>
public readonly record struct ChecklistTemplateId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistTemplateId wrapping a non-empty GUID.</summary>
    public ChecklistTemplateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistTemplateId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistTemplateId using Uuid7.</summary>
    public static ChecklistTemplateId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ChecklistTemplateId.</summary>
    public static ChecklistTemplateId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ChecklistTemplateId.</summary>
    public static ChecklistTemplateId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}