// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Observation aggregate root.</summary>
public readonly record struct ObservationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ObservationId wrapping a non-empty GUID.</summary>
    public ObservationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ObservationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ObservationId using Uuid7.</summary>
    public static ObservationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ObservationId.</summary>
    public static ObservationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ObservationId.</summary>
    public static ObservationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
