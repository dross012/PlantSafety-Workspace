// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ChecklistCompletion aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ChecklistCompletion aggregate root.</summary>
public readonly record struct ChecklistCompletionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistCompletionId wrapping a non-empty GUID.</summary>
    public ChecklistCompletionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistCompletionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistCompletionId using Uuid7.</summary>
    public static ChecklistCompletionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ChecklistCompletionId.</summary>
    public static ChecklistCompletionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ChecklistCompletionId.</summary>
    public static ChecklistCompletionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}