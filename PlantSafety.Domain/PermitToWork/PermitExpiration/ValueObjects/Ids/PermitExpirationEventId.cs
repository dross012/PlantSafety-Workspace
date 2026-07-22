// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PermitExpiration domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PermitExpiration domain events.</summary>
public readonly record struct PermitExpirationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitExpirationEventId wrapping a non-empty GUID.</summary>
    public PermitExpirationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitExpirationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitExpirationEventId using Uuid7.</summary>
    public static PermitExpirationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}