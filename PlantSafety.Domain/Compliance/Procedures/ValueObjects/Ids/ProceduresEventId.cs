// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Procedures domain events.</summary>
public readonly record struct ProceduresEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProceduresEventId wrapping a non-empty GUID.</summary>
    public ProceduresEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProceduresEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProceduresEventId using Uuid7.</summary>
    public static ProceduresEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
