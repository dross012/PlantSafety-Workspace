// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for LotoPrograms domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for LotoPrograms domain events.</summary>
public readonly record struct LotoProgramsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoProgramsEventId wrapping a non-empty GUID.</summary>
    public LotoProgramsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoProgramsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoProgramsEventId using Uuid7.</summary>
    public static LotoProgramsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}