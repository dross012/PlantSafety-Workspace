// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPEAssignment domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPEAssignment domain events.</summary>
public readonly record struct PPEAssignmentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEAssignmentEventId wrapping a non-empty GUID.</summary>
    public PPEAssignmentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEAssignmentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEAssignmentEventId using Uuid7.</summary>
    public static PPEAssignmentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}