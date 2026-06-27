// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           SiteId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Cross-cutting StrongId representing a physical plant/facility site.
//      Used across Operational, SafetyEvents, Compliance, and other modules.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.StrongIds;

/// <summary>Strong-typed site (plant/facility) identity used across modules.</summary>
public readonly record struct SiteId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SiteId wrapping a non-empty GUID.</summary>
    public SiteId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SiteId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SiteId using Uuid7.</summary>
    public static SiteId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SiteId.</summary>
    public static SiteId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SiteId.</summary>
    public static SiteId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
