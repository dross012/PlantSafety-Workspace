// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hierarchy
//  File:           ZoneId.cs
//  Created:        2026-08-08
//  Description:    Strongly typed identifier for a zone within an area in
//                  a FacilityHierarchy. Phase C.5 (Path C: Operational
//                  Modernization). Mirrors FacilityId's wrapper pattern.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hierarchy;

/// <summary>Strongly typed identifier for a zone within a <see cref="FacilityArea"/>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct ZoneId
{
    private ZoneId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="ZoneId"/>.</summary>
    public static ZoneId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as a <see cref="ZoneId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static ZoneId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ZoneId cannot be Guid.Empty.", nameof(value));
        }

        return new ZoneId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
