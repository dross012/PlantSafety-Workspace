// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Contractor domain events.</summary>
public readonly record struct ContractorEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ContractorEventId wrapping a non-empty GUID.</summary>
    public ContractorEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractorEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ContractorEventId using Uuid7.</summary>
    public static ContractorEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
