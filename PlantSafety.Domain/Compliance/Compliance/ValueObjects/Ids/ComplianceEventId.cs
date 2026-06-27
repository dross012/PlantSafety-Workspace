// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Compliance domain events.</summary>
public readonly record struct ComplianceEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ComplianceEventId wrapping a non-empty GUID.</summary>
    public ComplianceEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ComplianceEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ComplianceEventId using Uuid7.</summary>
    public static ComplianceEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
