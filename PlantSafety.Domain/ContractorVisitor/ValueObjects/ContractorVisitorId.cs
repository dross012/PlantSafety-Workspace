// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the ContractorVisitor aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the ContractorVisitor aggregate root.</summary>
public readonly record struct ContractorVisitorId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ContractorVisitorId wrapping a non-empty GUID.</summary>
    public ContractorVisitorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractorVisitorId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ContractorVisitorId using Uuid7.</summary>
    public static ContractorVisitorId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ContractorVisitorId.</summary>
    public static ContractorVisitorId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ContractorVisitorId.</summary>
    public static ContractorVisitorId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
