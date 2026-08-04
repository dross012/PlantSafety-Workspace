// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AutonomousVehicleIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AutonomousVehicleIncident aggregate root.</summary>
public readonly record struct AutonomousVehicleIncidentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutonomousVehicleIncidentId wrapping a non-empty GUID.</summary>
    public AutonomousVehicleIncidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutonomousVehicleIncidentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutonomousVehicleIncidentId using Uuid7.</summary>
    public static AutonomousVehicleIncidentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AutonomousVehicleIncidentId.</summary>
    public static AutonomousVehicleIncidentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AutonomousVehicleIncidentId.</summary>
    public static AutonomousVehicleIncidentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
