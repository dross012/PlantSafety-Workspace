// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the LotoProcedures aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LotoProcedures aggregate root.</summary>
public readonly record struct LotoProceduresId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoProceduresId wrapping a non-empty GUID.</summary>
    public LotoProceduresId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoProceduresId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoProceduresId using Uuid7.</summary>
    public static LotoProceduresId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LotoProceduresId.</summary>
    public static LotoProceduresId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LotoProceduresId.</summary>
    public static LotoProceduresId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}