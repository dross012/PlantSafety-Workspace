// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Contractor aggregate root.</summary>
public readonly record struct ContractorId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ContractorId wrapping a non-empty GUID.</summary>
    public ContractorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractorId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ContractorId using Uuid7.</summary>
    public static ContractorId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ContractorId.</summary>
    public static ContractorId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ContractorId.</summary>
    public static ContractorId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
