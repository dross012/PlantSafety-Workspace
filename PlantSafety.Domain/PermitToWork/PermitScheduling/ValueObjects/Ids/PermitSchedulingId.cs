// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PermitScheduling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PermitScheduling aggregate root.</summary>
public readonly record struct PermitSchedulingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitSchedulingId wrapping a non-empty GUID.</summary>
    public PermitSchedulingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitSchedulingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitSchedulingId using Uuid7.</summary>
    public static PermitSchedulingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PermitSchedulingId.</summary>
    public static PermitSchedulingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PermitSchedulingId.</summary>
    public static PermitSchedulingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}