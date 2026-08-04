// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for CraneHoistSystems domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CraneHoistSystems domain events.</summary>
public readonly record struct CraneHoistSystemsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CraneHoistSystemsEventId wrapping a non-empty GUID.</summary>
    public CraneHoistSystemsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CraneHoistSystemsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CraneHoistSystemsEventId using Uuid7.</summary>
    public static CraneHoistSystemsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
