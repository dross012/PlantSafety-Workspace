// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Checklists aggregate root.</summary>
public readonly record struct ChecklistsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ChecklistsId wrapping a non-empty GUID.</summary>
    public ChecklistsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChecklistsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ChecklistsId using Uuid7.</summary>
    public static ChecklistsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ChecklistsId.</summary>
    public static ChecklistsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ChecklistsId.</summary>
    public static ChecklistsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
