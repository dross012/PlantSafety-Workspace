// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EmergencyPreparedness aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EmergencyPreparedness aggregate root.</summary>
public readonly record struct EmergencyPreparednessId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyPreparednessId wrapping a non-empty GUID.</summary>
    public EmergencyPreparednessId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyPreparednessId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyPreparednessId using Uuid7.</summary>
    public static EmergencyPreparednessId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EmergencyPreparednessId.</summary>
    public static EmergencyPreparednessId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EmergencyPreparednessId.</summary>
    public static EmergencyPreparednessId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}