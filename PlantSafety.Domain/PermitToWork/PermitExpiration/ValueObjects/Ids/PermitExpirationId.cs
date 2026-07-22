// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PermitExpiration aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PermitExpiration aggregate root.</summary>
public readonly record struct PermitExpirationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitExpirationId wrapping a non-empty GUID.</summary>
    public PermitExpirationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitExpirationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitExpirationId using Uuid7.</summary>
    public static PermitExpirationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PermitExpirationId.</summary>
    public static PermitExpirationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PermitExpirationId.</summary>
    public static PermitExpirationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}