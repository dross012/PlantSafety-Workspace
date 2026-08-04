// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for SafetyCaseValidation domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SafetyCaseValidation domain events.</summary>
public readonly record struct SafetyCaseValidationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyCaseValidationEventId wrapping a non-empty GUID.</summary>
    public SafetyCaseValidationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyCaseValidationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyCaseValidationEventId using Uuid7.</summary>
    public static SafetyCaseValidationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
