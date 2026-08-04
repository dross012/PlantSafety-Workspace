// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PublicSafetyAgency aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PublicSafetyAgency aggregate root.</summary>
public readonly record struct PublicSafetyAgencyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PublicSafetyAgencyId wrapping a non-empty GUID.</summary>
    public PublicSafetyAgencyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PublicSafetyAgencyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PublicSafetyAgencyId using Uuid7.</summary>
    public static PublicSafetyAgencyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PublicSafetyAgencyId.</summary>
    public static PublicSafetyAgencyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PublicSafetyAgencyId.</summary>
    public static PublicSafetyAgencyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
