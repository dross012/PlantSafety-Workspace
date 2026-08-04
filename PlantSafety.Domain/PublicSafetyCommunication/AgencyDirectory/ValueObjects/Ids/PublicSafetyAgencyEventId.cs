// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PublicSafetyAgency domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PublicSafetyAgency domain events.</summary>
public readonly record struct PublicSafetyAgencyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PublicSafetyAgencyEventId wrapping a non-empty GUID.</summary>
    public PublicSafetyAgencyEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PublicSafetyAgencyEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PublicSafetyAgencyEventId using Uuid7.</summary>
    public static PublicSafetyAgencyEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
