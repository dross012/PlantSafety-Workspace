// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ManagementOfChange aggregate root.</summary>
public readonly record struct ManagementOfChangeId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ManagementOfChangeId wrapping a non-empty GUID.</summary>
    public ManagementOfChangeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ManagementOfChangeId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ManagementOfChangeId using Uuid7.</summary>
    public static ManagementOfChangeId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ManagementOfChangeId.</summary>
    public static ManagementOfChangeId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ManagementOfChangeId.</summary>
    public static ManagementOfChangeId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
