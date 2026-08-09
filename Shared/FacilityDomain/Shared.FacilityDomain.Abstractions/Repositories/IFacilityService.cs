// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Repositories
//  File:           IFacilityService.cs
//  Created:        2026-08-08
//  Description:    The outward-facing application service over
//                  FacilityAggregate/IFacilityRepository -- callers outside
//                  Shared.FacilityDomain (future Reporting.Api/UI wiring)
//                  depend on this, not on the aggregate or repository
//                  directly.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Repositories;

/// <summary>Application service orchestrating FacilityAggregate commands against IFacilityRepository.</summary>
public interface IFacilityService
{
    /// <summary>Creates a new facility and returns its resulting state.</summary>
    Task<FacilityState> CreateFacilityAsync(string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing facility's metadata. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> UpdateFacilityMetadataAsync(FacilityId facilityId, string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing facility's hazard profile. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> UpdateHazardProfileAsync(FacilityId facilityId, IReadOnlyList<HazardType> activeHazardTypes, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing facility's regulatory profile. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> UpdateRegulatoryProfileAsync(FacilityId facilityId, IReadOnlyList<string> regulatorCodes, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing facility's program profile. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> UpdateProgramProfileAsync(FacilityId facilityId, IReadOnlyList<string> requiredProgramCodes, CancellationToken cancellationToken = default);

    /// <summary>Adds a new building to an existing facility's hierarchy. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> AddBuildingAsync(FacilityId facilityId, string name, CancellationToken cancellationToken = default);

    /// <summary>Adds a new area to an existing building. Returns null if no facility with that id exists.</summary>
    /// <exception cref="InvalidOperationException">No building with <paramref name="buildingId"/> exists.</exception>
    Task<FacilityState?> AddAreaAsync(FacilityId facilityId, BuildingId buildingId, string name, CancellationToken cancellationToken = default);

    /// <summary>Adds a new zone to an existing area. Returns null if no facility with that id exists.</summary>
    /// <exception cref="InvalidOperationException">No area with <paramref name="areaId"/> exists.</exception>
    Task<FacilityState?> AddZoneAsync(FacilityId facilityId, AreaId areaId, string name, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing facility's entire hierarchy wholesale. Returns null if no facility with that id exists.</summary>
    Task<FacilityState?> UpdateHierarchyAsync(FacilityId facilityId, FacilityHierarchy hierarchy, CancellationToken cancellationToken = default);

    /// <summary>Returns the facility with the given id, or null if none exists.</summary>
    Task<FacilityState?> GetFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default);

    /// <summary>Returns every facility, in no particular order.</summary>
    Task<IReadOnlyList<FacilityState>> GetAllFacilitiesAsync(CancellationToken cancellationToken = default);
}
