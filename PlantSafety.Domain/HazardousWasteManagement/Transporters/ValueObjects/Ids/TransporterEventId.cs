// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for Transporter domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Transporter domain events.</summary>
public readonly record struct TransporterEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransporterEventId wrapping a non-empty GUID.</summary>
    public TransporterEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransporterEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransporterEventId using Uuid7.</summary>
    public static TransporterEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
