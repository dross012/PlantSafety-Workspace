// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPEManagement domain events.</summary>
public readonly record struct PPEManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEManagementEventId wrapping a non-empty GUID.</summary>
    public PPEManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEManagementEventId using Uuid7.</summary>
    public static PPEManagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
