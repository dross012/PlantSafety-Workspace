// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Identity
//  Layer:          Domain
//  File:           PartnerIdentityMappingId.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the PartnerIdentityMapping
//                  aggregate root (Phase 6 §2.2 "Tenant Federation" — Partner
//                  identity mapping). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Identity.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the PartnerIdentityMapping aggregate root.</summary>
public readonly record struct PartnerIdentityMappingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PartnerIdentityMappingId wrapping a non-empty GUID.</summary>
    public PartnerIdentityMappingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PartnerIdentityMappingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PartnerIdentityMappingId using Uuid7.</summary>
    public static PartnerIdentityMappingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PartnerIdentityMappingId.</summary>
    public static PartnerIdentityMappingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PartnerIdentityMappingId.</summary>
    public static PartnerIdentityMappingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
