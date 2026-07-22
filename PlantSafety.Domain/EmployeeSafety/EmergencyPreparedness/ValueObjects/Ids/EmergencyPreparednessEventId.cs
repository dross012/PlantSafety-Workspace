// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EmergencyPreparedness domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EmergencyPreparedness domain events.</summary>
public readonly record struct EmergencyPreparednessEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyPreparednessEventId wrapping a non-empty GUID.</summary>
    public EmergencyPreparednessEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyPreparednessEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyPreparednessEventId using Uuid7.</summary>
    public static EmergencyPreparednessEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}