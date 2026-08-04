// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LiaisonLogEntryId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the LiaisonLogEntry child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a <see cref="Entities.LiaisonLogEntry"/> child entity.</summary>
public readonly record struct LiaisonLogEntryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LiaisonLogEntryId wrapping a non-empty GUID.</summary>
    public LiaisonLogEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LiaisonLogEntryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LiaisonLogEntryId using Uuid7.</summary>
    public static LiaisonLogEntryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LiaisonLogEntryId.</summary>
    public static LiaisonLogEntryId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
