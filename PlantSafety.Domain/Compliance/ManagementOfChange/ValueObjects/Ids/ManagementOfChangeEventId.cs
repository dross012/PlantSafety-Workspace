// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ManagementOfChange domain events.</summary>
public readonly record struct ManagementOfChangeEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ManagementOfChangeEventId wrapping a non-empty GUID.</summary>
    public ManagementOfChangeEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ManagementOfChangeEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ManagementOfChangeEventId using Uuid7.</summary>
    public static ManagementOfChangeEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
