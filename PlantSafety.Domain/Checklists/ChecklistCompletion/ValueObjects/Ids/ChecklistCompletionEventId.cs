// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for ChecklistCompletion domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ChecklistCompletion domain events.</summary>
public readonly record struct ChecklistCompletionEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistCompletionEventId wrapping a non-empty GUID.</summary>
    public ChecklistCompletionEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistCompletionEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistCompletionEventId using Uuid7.</summary>
    public static ChecklistCompletionEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}