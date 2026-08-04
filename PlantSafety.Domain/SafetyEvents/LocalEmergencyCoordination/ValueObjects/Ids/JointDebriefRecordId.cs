// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           JointDebriefRecordId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the JointDebriefRecord child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a <see cref="Entities.JointDebriefRecord"/> child entity.</summary>
public readonly record struct JointDebriefRecordId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a JointDebriefRecordId wrapping a non-empty GUID.</summary>
    public JointDebriefRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("JointDebriefRecordId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new JointDebriefRecordId using Uuid7.</summary>
    public static JointDebriefRecordId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a JointDebriefRecordId.</summary>
    public static JointDebriefRecordId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
