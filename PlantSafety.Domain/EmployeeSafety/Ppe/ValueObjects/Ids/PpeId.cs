// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Ppe aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Ppe aggregate root.</summary>
public readonly record struct PpeId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PpeId wrapping a non-empty GUID.</summary>
    public PpeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PpeId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PpeId using Uuid7.</summary>
    public static PpeId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PpeId.</summary>
    public static PpeId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PpeId.</summary>
    public static PpeId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}