// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Permits domain events.</summary>
public readonly record struct PermitsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitsEventId wrapping a non-empty GUID.</summary>
    public PermitsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitsEventId using Uuid7.</summary>
    public static PermitsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
