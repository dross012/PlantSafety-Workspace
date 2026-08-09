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

    /// <summary>Returns the facility with the given id, or null if none exists.</summary>
    Task<FacilityState?> GetFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default);

    /// <summary>Returns every facility, in no particular order.</summary>
    Task<IReadOnlyList<FacilityState>> GetAllFacilitiesAsync(CancellationToken cancellationToken = default);
}
