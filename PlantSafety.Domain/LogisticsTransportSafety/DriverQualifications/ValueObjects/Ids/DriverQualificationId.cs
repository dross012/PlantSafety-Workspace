// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DriverQualification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DriverQualification aggregate root.</summary>
public readonly record struct DriverQualificationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DriverQualificationId wrapping a non-empty GUID.</summary>
    public DriverQualificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DriverQualificationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DriverQualificationId using Uuid7.</summary>
    public static DriverQualificationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DriverQualificationId.</summary>
    public static DriverQualificationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DriverQualificationId.</summary>
    public static DriverQualificationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
