// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for LocalEmergencyCoordination domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for LocalEmergencyCoordination domain events.</summary>
public readonly record struct LocalEmergencyCoordinationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LocalEmergencyCoordinationEventId wrapping a non-empty GUID.</summary>
    public LocalEmergencyCoordinationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LocalEmergencyCoordinationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LocalEmergencyCoordinationEventId using Uuid7.</summary>
    public static LocalEmergencyCoordinationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
