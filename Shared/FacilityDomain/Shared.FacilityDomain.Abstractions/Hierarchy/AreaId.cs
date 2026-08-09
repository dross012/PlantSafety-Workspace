// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hierarchy
//  File:           AreaId.cs
//  Created:        2026-08-08
//  Description:    Strongly typed identifier for an area within a building
//                  in a FacilityHierarchy. Phase C.5 (Path C: Operational
//                  Modernization). Mirrors FacilityId's wrapper pattern.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hierarchy;

/// <summary>Strongly typed identifier for an area within a <see cref="FacilityBuilding"/>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct AreaId
{
    private AreaId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="AreaId"/>.</summary>
    public static AreaId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as an <see cref="AreaId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static AreaId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AreaId cannot be Guid.Empty.", nameof(value));
        }

        return new AreaId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
