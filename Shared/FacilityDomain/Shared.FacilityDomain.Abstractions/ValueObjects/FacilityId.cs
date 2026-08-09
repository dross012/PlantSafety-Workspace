// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  Layer:          Abstractions
//  File:           FacilityId.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strongly typed identifier for a Facility aggregate.
//                  Phase B.1 (Path B: Transformative Modernization) --
//                  the first real Facility identity in the PlantSafety
//                  codebase: every prior "facility" concept (Shared.
//                  SafetyData's FacilityProfile, TenantFacilityRegistration)
//                  was a read-model/interim-correlation shape layered over
//                  TenantId, precisely because no real Facility aggregate
//                  existed. This type -- and the aggregate it identifies --
//                  is that real aggregate.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>Strongly typed identifier for a <c>FacilityAggregate</c>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct FacilityId
{
    private FacilityId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="FacilityId"/>.</summary>
    public static FacilityId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as a <see cref="FacilityId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static FacilityId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FacilityId cannot be Guid.Empty.", nameof(value));
        }

        return new FacilityId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
