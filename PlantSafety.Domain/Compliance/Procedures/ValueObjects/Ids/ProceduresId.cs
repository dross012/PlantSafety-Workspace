// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Procedures aggregate root.</summary>
public readonly record struct ProceduresId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProceduresId wrapping a non-empty GUID.</summary>
    public ProceduresId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProceduresId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProceduresId using Uuid7.</summary>
    public static ProceduresId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ProceduresId.</summary>
    public static ProceduresId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ProceduresId.</summary>
    public static ProceduresId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
