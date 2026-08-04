// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EgressSimulationRun domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EgressSimulationRun domain events.</summary>
public readonly record struct EgressSimulationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EgressSimulationEventId wrapping a non-empty GUID.</summary>
    public EgressSimulationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EgressSimulationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EgressSimulationEventId using Uuid7.</summary>
    public static EgressSimulationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
