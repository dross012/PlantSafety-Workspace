// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PermitScheduling domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PermitScheduling domain events.</summary>
public readonly record struct PermitSchedulingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitSchedulingEventId wrapping a non-empty GUID.</summary>
    public PermitSchedulingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitSchedulingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitSchedulingEventId using Uuid7.</summary>
    public static PermitSchedulingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}