// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Ppe domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Ppe domain events.</summary>
public readonly record struct PpeEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PpeEventId wrapping a non-empty GUID.</summary>
    public PpeEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PpeEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PpeEventId using Uuid7.</summary>
    public static PpeEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}