// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyCaseValidation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyCaseValidation aggregate root.</summary>
public readonly record struct SafetyCaseValidationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyCaseValidationId wrapping a non-empty GUID.</summary>
    public SafetyCaseValidationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyCaseValidationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyCaseValidationId using Uuid7.</summary>
    public static SafetyCaseValidationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyCaseValidationId.</summary>
    public static SafetyCaseValidationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SafetyCaseValidationId.</summary>
    public static SafetyCaseValidationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
