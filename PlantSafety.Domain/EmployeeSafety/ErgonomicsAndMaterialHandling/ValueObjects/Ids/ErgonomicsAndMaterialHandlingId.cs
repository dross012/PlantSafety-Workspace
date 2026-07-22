// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ErgonomicsAndMaterialHandling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ErgonomicsAndMaterialHandling aggregate root.</summary>
public readonly record struct ErgonomicsAndMaterialHandlingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ErgonomicsAndMaterialHandlingId wrapping a non-empty GUID.</summary>
    public ErgonomicsAndMaterialHandlingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ErgonomicsAndMaterialHandlingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ErgonomicsAndMaterialHandlingId using Uuid7.</summary>
    public static ErgonomicsAndMaterialHandlingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ErgonomicsAndMaterialHandlingId.</summary>
    public static ErgonomicsAndMaterialHandlingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ErgonomicsAndMaterialHandlingId.</summary>
    public static ErgonomicsAndMaterialHandlingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}