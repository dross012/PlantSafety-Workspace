// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the RiskManagement aggregate root.</summary>
public readonly record struct RiskManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RiskManagementId wrapping a non-empty GUID.</summary>
    public RiskManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RiskManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RiskManagementId using Uuid7.</summary>
    public static RiskManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RiskManagementId.</summary>
    public static RiskManagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RiskManagementId.</summary>
    public static RiskManagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
