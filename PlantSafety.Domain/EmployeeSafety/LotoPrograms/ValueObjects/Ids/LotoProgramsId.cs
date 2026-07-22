// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the LotoPrograms aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LotoPrograms aggregate root.</summary>
public readonly record struct LotoProgramsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoProgramsId wrapping a non-empty GUID.</summary>
    public LotoProgramsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoProgramsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoProgramsId using Uuid7.</summary>
    public static LotoProgramsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LotoProgramsId.</summary>
    public static LotoProgramsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LotoProgramsId.</summary>
    public static LotoProgramsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}