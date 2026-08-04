// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DriverQualification domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DriverQualification domain events.</summary>
public readonly record struct DriverQualificationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DriverQualificationEventId wrapping a non-empty GUID.</summary>
    public DriverQualificationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DriverQualificationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DriverQualificationEventId using Uuid7.</summary>
    public static DriverQualificationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
