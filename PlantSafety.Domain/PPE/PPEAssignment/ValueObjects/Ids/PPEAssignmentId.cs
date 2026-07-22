// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPEAssignment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPEAssignment aggregate root.</summary>
public readonly record struct PPEAssignmentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEAssignmentId wrapping a non-empty GUID.</summary>
    public PPEAssignmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEAssignmentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEAssignmentId using Uuid7.</summary>
    public static PPEAssignmentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPEAssignmentId.</summary>
    public static PPEAssignmentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPEAssignmentId.</summary>
    public static PPEAssignmentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}