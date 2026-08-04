// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           AgencyContactId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AgencyContact child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AgencyContact child entity.</summary>
public readonly record struct AgencyContactId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AgencyContactId wrapping a non-empty GUID.</summary>
    public AgencyContactId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AgencyContactId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AgencyContactId using Uuid7.</summary>
    public static AgencyContactId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AgencyContactId.</summary>
    public static AgencyContactId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
