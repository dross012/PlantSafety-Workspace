// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the LocalEmergencyCoordination aggregate root.
//                  Mirrors PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids.EmergencyManagementId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LocalEmergencyCoordination aggregate root.</summary>
public readonly record struct LocalEmergencyCoordinationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LocalEmergencyCoordinationId wrapping a non-empty GUID.</summary>
    public LocalEmergencyCoordinationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LocalEmergencyCoordinationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LocalEmergencyCoordinationId using Uuid7.</summary>
    public static LocalEmergencyCoordinationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LocalEmergencyCoordinationId.</summary>
    public static LocalEmergencyCoordinationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LocalEmergencyCoordinationId.</summary>
    public static LocalEmergencyCoordinationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
