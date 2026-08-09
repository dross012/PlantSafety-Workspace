// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hierarchy
//  File:           BuildingId.cs
//  Created:        2026-08-08
//  Description:    Strongly typed identifier for a building within a
//                  FacilityHierarchy. Phase C.5 (Path C: Operational
//                  Modernization). Mirrors FacilityId's wrapper pattern.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hierarchy;

/// <summary>Strongly typed identifier for a building within a <see cref="FacilityHierarchy"/>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct BuildingId
{
    private BuildingId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="BuildingId"/>.</summary>
    public static BuildingId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as a <see cref="BuildingId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static BuildingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BuildingId cannot be Guid.Empty.", nameof(value));
        }

        return new BuildingId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
