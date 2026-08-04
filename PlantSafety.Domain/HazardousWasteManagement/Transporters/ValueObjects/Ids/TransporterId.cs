// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the Transporter aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Transporter aggregate root.</summary>
public readonly record struct TransporterId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransporterId wrapping a non-empty GUID.</summary>
    public TransporterId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransporterId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransporterId using Uuid7.</summary>
    public static TransporterId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TransporterId.</summary>
    public static TransporterId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TransporterId.</summary>
    public static TransporterId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
