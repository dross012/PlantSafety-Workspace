// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AutonomousVehicleIncident domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AutonomousVehicleIncident domain events.</summary>
public readonly record struct AutonomousVehicleIncidentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutonomousVehicleIncidentEventId wrapping a non-empty GUID.</summary>
    public AutonomousVehicleIncidentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutonomousVehicleIncidentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutonomousVehicleIncidentEventId using Uuid7.</summary>
    public static AutonomousVehicleIncidentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
