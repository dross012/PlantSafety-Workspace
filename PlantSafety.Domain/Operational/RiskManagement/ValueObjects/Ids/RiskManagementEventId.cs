// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for RiskManagement domain events.</summary>
public readonly record struct RiskManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RiskManagementEventId wrapping a non-empty GUID.</summary>
    public RiskManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RiskManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RiskManagementEventId using Uuid7.</summary>
    public static RiskManagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
