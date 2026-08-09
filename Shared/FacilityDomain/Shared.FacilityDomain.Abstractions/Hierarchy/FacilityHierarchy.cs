// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hierarchy
//  File:           FacilityHierarchy.cs
//  Created:        2026-08-08
//  Description:    Phase C.5 (Path C: Operational Modernization): the
//                  three-level physical hierarchy (Building -> Area ->
//                  Zone) within a facility. FacilityBuilding/FacilityArea/
//                  FacilityZone are defined alongside FacilityHierarchy,
//                  not as separate files, since Path C's file list names
//                  only FacilityHierarchy.cs -- they are the hierarchy's
//                  internal shape, not independently addressable value
//                  objects. Every level is added via FacilityAggregate
//                  commands; nothing here is mutable.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hierarchy;

/// <summary>A zone: the innermost level of a <see cref="FacilityHierarchy"/>.</summary>
public sealed record FacilityZone(ZoneId ZoneId, string Name);

/// <summary>An area within a <see cref="FacilityBuilding"/>, containing zero or more zones.</summary>
public sealed record FacilityArea(AreaId AreaId, string Name, IReadOnlyList<FacilityZone> Zones);

/// <summary>A building within a facility, containing zero or more areas.</summary>
public sealed record FacilityBuilding(BuildingId BuildingId, string Name, IReadOnlyList<FacilityArea> Areas);

/// <summary>The physical Building/Area/Zone hierarchy of a facility, as of <see cref="UpdatedAtUtc"/>.</summary>
public sealed record FacilityHierarchy(IReadOnlyList<FacilityBuilding> Buildings, DateTime UpdatedAtUtc)
{
    /// <summary>An empty hierarchy, for a newly created facility.</summary>
    public static FacilityHierarchy Empty(DateTime nowUtc) => new([], nowUtc);
}
