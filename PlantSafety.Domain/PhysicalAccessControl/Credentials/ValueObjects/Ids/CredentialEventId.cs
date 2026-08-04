// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Credential domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Credential domain events.</summary>
public readonly record struct CredentialEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CredentialEventId wrapping a non-empty GUID.</summary>
    public CredentialEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CredentialEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CredentialEventId using Uuid7.</summary>
    public static CredentialEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
