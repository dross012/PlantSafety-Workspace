// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPEManagement aggregate root.</summary>
public readonly record struct PPEManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEManagementId wrapping a non-empty GUID.</summary>
    public PPEManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEManagementId using Uuid7.</summary>
    public static PPEManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPEManagementId.</summary>
    public static PPEManagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPEManagementId.</summary>
    public static PPEManagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
