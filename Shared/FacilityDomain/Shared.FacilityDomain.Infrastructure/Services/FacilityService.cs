// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Services
//  File:           FacilityService.cs
//  Created:        2026-08-08
//  Description:    IFacilityService implementation: loads (or creates) a
//                  FacilityAggregate, invokes the requested command,
//                  persists the result via IFacilityRepository, and clears
//                  the aggregate's pending domain events -- nothing
//                  publishes them yet. Real publishing (through
//                  EventEnvelopeV2) is Phase B.4's job, not this one's; see
//                  FacilityAggregate.cs's header for why the events are
//                  still collected even though nothing consumes them yet.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Infrastructure.Services;

/// <summary><see cref="IFacilityService"/> implementation over <see cref="IFacilityRepository"/>.</summary>
public sealed class FacilityService(IFacilityRepository repository) : IFacilityService
{
    /// <inheritdoc />
    public async Task<FacilityState> CreateFacilityAsync(string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default)
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), name, industry, location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateFacilityMetadataAsync(FacilityId facilityId, string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateFacilityMetadata(name, industry, location, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateHazardProfileAsync(FacilityId facilityId, IReadOnlyList<HazardType> activeHazardTypes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateHazardProfile(activeHazardTypes, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateRegulatoryProfileAsync(FacilityId facilityId, IReadOnlyList<string> regulatorCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateRegulatoryProfile(regulatorCodes, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateProgramProfileAsync(FacilityId facilityId, IReadOnlyList<string> requiredProgramCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateProgramProfile(requiredProgramCodes, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddBuildingAsync(FacilityId facilityId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddBuilding(BuildingId.New(), name, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddAreaAsync(FacilityId facilityId, BuildingId buildingId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddArea(buildingId, AreaId.New(), name, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddZoneAsync(FacilityId facilityId, AreaId areaId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddZone(areaId, ZoneId.New(), name, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateHierarchyAsync(FacilityId facilityId, FacilityHierarchy hierarchy, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateHierarchy(hierarchy, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ActivateFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ActivateFacility(DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ShutdownFacilityAsync(FacilityId facilityId, string reason, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ShutdownFacility(reason, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ReopenFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ReopenFacility(DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> DecommissionFacilityAsync(FacilityId facilityId, string reason, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.DecommissionFacility(reason, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> GetFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default)
    {
        FacilityAggregate? aggregate = await repository.FindAsync(facilityId, cancellationToken);
        return aggregate?.State;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FacilityState>> GetAllFacilitiesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FacilityAggregate> aggregates = await repository.GetAllAsync(cancellationToken);
        return aggregates.Select(aggregate => aggregate.State).ToArray();
    }

    private async Task<FacilityState?> ApplyUpdateAsync(FacilityId facilityId, Action<FacilityAggregate> applyCommand, CancellationToken cancellationToken)
    {
        FacilityAggregate? aggregate = await repository.FindAsync(facilityId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        applyCommand(aggregate);
        await repository.SaveAsync(aggregate, cancellationToken);
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }
}
