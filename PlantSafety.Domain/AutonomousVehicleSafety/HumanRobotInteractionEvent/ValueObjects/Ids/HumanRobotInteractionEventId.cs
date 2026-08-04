// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HumanRobotInteractionEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HumanRobotInteractionEvent aggregate root.</summary>
public readonly record struct HumanRobotInteractionEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HumanRobotInteractionEventId wrapping a non-empty GUID.</summary>
    public HumanRobotInteractionEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HumanRobotInteractionEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HumanRobotInteractionEventId using Uuid7.</summary>
    public static HumanRobotInteractionEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HumanRobotInteractionEventId.</summary>
    public static HumanRobotInteractionEventId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HumanRobotInteractionEventId.</summary>
    public static HumanRobotInteractionEventId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
