// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Observation domain events.</summary>
public readonly record struct ObservationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ObservationEventId wrapping a non-empty GUID.</summary>
    public ObservationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ObservationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ObservationEventId using Uuid7.</summary>
    public static ObservationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
