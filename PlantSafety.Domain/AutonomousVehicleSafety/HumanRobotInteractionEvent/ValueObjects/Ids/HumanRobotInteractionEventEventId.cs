// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for HumanRobotInteractionEvent domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HumanRobotInteractionEvent domain events.</summary>
public readonly record struct HumanRobotInteractionEventEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HumanRobotInteractionEventEventId wrapping a non-empty GUID.</summary>
    public HumanRobotInteractionEventEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HumanRobotInteractionEventEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HumanRobotInteractionEventEventId using Uuid7.</summary>
    public static HumanRobotInteractionEventEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
