// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EgressSimulationRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EgressSimulationRun aggregate root.</summary>
public readonly record struct EgressSimulationRunId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EgressSimulationRunId wrapping a non-empty GUID.</summary>
    public EgressSimulationRunId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EgressSimulationRunId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EgressSimulationRunId using Uuid7.</summary>
    public static EgressSimulationRunId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an EgressSimulationRunId.</summary>
    public static EgressSimulationRunId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an EgressSimulationRunId.</summary>
    public static EgressSimulationRunId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
