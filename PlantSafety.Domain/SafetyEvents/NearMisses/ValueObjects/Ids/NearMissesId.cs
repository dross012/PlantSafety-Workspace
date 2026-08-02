// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the NearMisses aggregate root.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (NearMissesModuleMarker) and a read-only
//                  query slice over a "NearMisses" table existed. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids.PermitsId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;

/// <summary>Strong-typed identity for the NearMisses aggregate root.</summary>
public readonly record struct NearMissesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a NearMissesId wrapping a non-empty GUID.</summary>
    public NearMissesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("NearMissesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new NearMissesId using Uuid7.</summary>
    public static NearMissesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a NearMissesId.</summary>
    public static NearMissesId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
