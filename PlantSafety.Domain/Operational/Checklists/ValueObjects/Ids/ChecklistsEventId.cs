// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Checklists domain events.</summary>
public readonly record struct ChecklistsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistsEventId wrapping a non-empty GUID.</summary>
    public ChecklistsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistsEventId using Uuid7.</summary>
    public static ChecklistsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
