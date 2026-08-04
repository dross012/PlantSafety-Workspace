// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Credential aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Credential aggregate root.</summary>
public readonly record struct CredentialId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CredentialId wrapping a non-empty GUID.</summary>
    public CredentialId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CredentialId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CredentialId using Uuid7.</summary>
    public static CredentialId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CredentialId.</summary>
    public static CredentialId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CredentialId.</summary>
    public static CredentialId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
