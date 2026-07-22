// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for LotoProcedures domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for LotoProcedures domain events.</summary>
public readonly record struct LotoProceduresEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoProceduresEventId wrapping a non-empty GUID.</summary>
    public LotoProceduresEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoProceduresEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoProceduresEventId using Uuid7.</summary>
    public static LotoProceduresEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}