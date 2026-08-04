// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the CraneHoistSystems aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CraneHoistSystems aggregate root.</summary>
public readonly record struct CraneHoistSystemsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CraneHoistSystemsId wrapping a non-empty GUID.</summary>
    public CraneHoistSystemsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CraneHoistSystemsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CraneHoistSystemsId using Uuid7.</summary>
    public static CraneHoistSystemsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CraneHoistSystemsId.</summary>
    public static CraneHoistSystemsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CraneHoistSystemsId.</summary>
    public static CraneHoistSystemsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
