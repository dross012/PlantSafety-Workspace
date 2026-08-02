// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for NearMisses domain events. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids.PermitsEventId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for NearMisses domain events.</summary>
public readonly record struct NearMissesEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a NearMissesEventId wrapping a non-empty GUID.</summary>
    public NearMissesEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("NearMissesEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new NearMissesEventId using Uuid7.</summary>
    public static NearMissesEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
