// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Aggregates
//  File:           FacilityAggregate.cs
//  Created:        2026-08-08
//  Description:    The Facility aggregate root. Owns a FacilityState
//                  snapshot, replaced immutably (via `with`) by every
//                  command, and collects the domain event each command
//                  raises so a caller (FacilityService, in the
//                  Infrastructure layer) can persist the new state and
//                  (from Phase B.4 onward) publish the events. No
//                  reflection, no dynamic typing -- every command is a
//                  plain, fully typed method.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Events;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Aggregates;

/// <summary>The Facility aggregate root.</summary>
public sealed class FacilityAggregate
{
    private readonly List<object> _domainEvents = [];

    private FacilityAggregate(FacilityState state) => State = state;

    /// <summary>The aggregate's current, immutable state snapshot.</summary>
    public FacilityState State { get; private set; }

    /// <summary>Domain events raised since construction/rehydration, in the order they occurred.</summary>
    public IReadOnlyList<object> DomainEvents => _domainEvents;

    /// <summary>Creates a new facility. Its hazard/regulatory/program profiles start empty.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public static FacilityAggregate CreateFacility(FacilityId facilityId, string name, FacilityIndustry industry, FacilityLocation location, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name is required.", nameof(name));
        }

        var state = new FacilityState(
            FacilityId: facilityId,
            Name: name,
            Industry: industry,
            Location: location,
            HazardProfile: new FacilityHazardProfile([], nowUtc),
            RegulatoryProfile: new FacilityRegulatoryProfile([], nowUtc),
            ProgramProfile: new FacilityProgramProfile([], nowUtc),
            Hierarchy: FacilityHierarchy.Empty(nowUtc),
            LifecycleStatus: FacilityLifecycleStatus.PendingActivation,
            CreatedAtUtc: nowUtc,
            UpdatedAtUtc: nowUtc);

        var aggregate = new FacilityAggregate(state);
        aggregate._domainEvents.Add(new FacilityCreatedEvent(facilityId, name, industry, location, nowUtc));
        return aggregate;
    }

    /// <summary>Rehydrates an aggregate from a previously persisted state, with no domain events pending.</summary>
    public static FacilityAggregate Rehydrate(FacilityState state) => new(state);

    /// <summary>Updates the facility's name/industry/location.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public void UpdateFacilityMetadata(string name, FacilityIndustry industry, FacilityLocation location, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name is required.", nameof(name));
        }

        State = State with { Name = name, Industry = industry, Location = location, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityUpdatedEvent(State.FacilityId, FacilityUpdateKind.Metadata, nowUtc));
    }

    /// <summary>Replaces the facility's active hazard types.</summary>
    public void UpdateHazardProfile(IReadOnlyList<HazardType> activeHazardTypes, DateTime nowUtc)
    {
        State = State with { HazardProfile = new FacilityHazardProfile(activeHazardTypes, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityUpdatedEvent(State.FacilityId, FacilityUpdateKind.HazardProfile, nowUtc));
    }

    /// <summary>Replaces the facility's bound regulator codes.</summary>
    public void UpdateRegulatoryProfile(IReadOnlyList<string> regulatorCodes, DateTime nowUtc)
    {
        State = State with { RegulatoryProfile = new FacilityRegulatoryProfile(regulatorCodes, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityUpdatedEvent(State.FacilityId, FacilityUpdateKind.RegulatoryProfile, nowUtc));
    }

    /// <summary>Replaces the facility's required program codes.</summary>
    public void UpdateProgramProfile(IReadOnlyList<string> requiredProgramCodes, DateTime nowUtc)
    {
        State = State with { ProgramProfile = new FacilityProgramProfile(requiredProgramCodes, nowUtc), UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityUpdatedEvent(State.FacilityId, FacilityUpdateKind.ProgramProfile, nowUtc));
    }

    /// <summary>Adds a new, empty building to the facility's hierarchy.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public void AddBuilding(BuildingId buildingId, string name, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Building name is required.", nameof(name));
        }

        var building = new FacilityBuilding(buildingId, name, []);
        State = State with { Hierarchy = State.Hierarchy with { Buildings = [.. State.Hierarchy.Buildings, building], UpdatedAtUtc = nowUtc }, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityHierarchyUpdatedEvent(State.FacilityId, FacilityHierarchyChangeKind.BuildingAdded, nowUtc));
    }

    /// <summary>Adds a new, empty area to an existing building.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">No building with <paramref name="buildingId"/> exists.</exception>
    public void AddArea(BuildingId buildingId, AreaId areaId, string name, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Area name is required.", nameof(name));
        }

        var area = new FacilityArea(areaId, name, []);
        IReadOnlyList<FacilityBuilding> buildings = State.Hierarchy.Buildings
            .Select(b => b.BuildingId == buildingId ? b with { Areas = [.. b.Areas, area] } : b)
            .ToArray();

        if (!buildings.Any(b => b.BuildingId == buildingId))
        {
            throw new InvalidOperationException($"No building with id '{buildingId}' exists on this facility.");
        }

        State = State with { Hierarchy = State.Hierarchy with { Buildings = buildings, UpdatedAtUtc = nowUtc }, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityHierarchyUpdatedEvent(State.FacilityId, FacilityHierarchyChangeKind.AreaAdded, nowUtc));
    }

    /// <summary>Adds a new zone to an existing area.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">No area with <paramref name="areaId"/> exists.</exception>
    public void AddZone(AreaId areaId, ZoneId zoneId, string name, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Zone name is required.", nameof(name));
        }

        var zone = new FacilityZone(zoneId, name);
        bool areaFound = false;
        IReadOnlyList<FacilityBuilding> buildings = State.Hierarchy.Buildings
            .Select(building => building with
            {
                Areas = building.Areas.Select(area =>
                {
                    if (area.AreaId != areaId)
                    {
                        return area;
                    }

                    areaFound = true;
                    return area with { Zones = [.. area.Zones, zone] };
                }).ToArray(),
            })
            .ToArray();

        if (!areaFound)
        {
            throw new InvalidOperationException($"No area with id '{areaId}' exists on this facility.");
        }

        State = State with { Hierarchy = State.Hierarchy with { Buildings = buildings, UpdatedAtUtc = nowUtc }, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityHierarchyUpdatedEvent(State.FacilityId, FacilityHierarchyChangeKind.ZoneAdded, nowUtc));
    }

    /// <summary>Replaces the facility's entire hierarchy wholesale.</summary>
    public void UpdateHierarchy(FacilityHierarchy hierarchy, DateTime nowUtc)
    {
        State = State with { Hierarchy = hierarchy with { UpdatedAtUtc = nowUtc }, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityHierarchyUpdatedEvent(State.FacilityId, FacilityHierarchyChangeKind.Replaced, nowUtc));
    }

    /// <summary>Activates a pending facility for operation.</summary>
    /// <exception cref="InvalidOperationException">The facility is not in <see cref="FacilityLifecycleStatus.PendingActivation"/>.</exception>
    public void ActivateFacility(DateTime nowUtc)
    {
        if (State.LifecycleStatus != FacilityLifecycleStatus.PendingActivation)
        {
            throw new InvalidOperationException($"Cannot activate a facility in {State.LifecycleStatus} status.");
        }

        State = State with { LifecycleStatus = FacilityLifecycleStatus.Active, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityActivatedEvent(State.FacilityId, nowUtc));
    }

    /// <summary>Temporarily shuts down an active facility.</summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">The facility is not <see cref="FacilityLifecycleStatus.Active"/>.</exception>
    public void ShutdownFacility(string reason, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Shutdown reason is required.", nameof(reason));
        }
        if (State.LifecycleStatus != FacilityLifecycleStatus.Active)
        {
            throw new InvalidOperationException($"Cannot shut down a facility in {State.LifecycleStatus} status.");
        }

        State = State with { LifecycleStatus = FacilityLifecycleStatus.ShutDown, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityShutdownEvent(State.FacilityId, reason, nowUtc));
    }

    /// <summary>Reopens a shut-down facility.</summary>
    /// <exception cref="InvalidOperationException">The facility is not <see cref="FacilityLifecycleStatus.ShutDown"/>.</exception>
    public void ReopenFacility(DateTime nowUtc)
    {
        if (State.LifecycleStatus != FacilityLifecycleStatus.ShutDown)
        {
            throw new InvalidOperationException($"Cannot reopen a facility in {State.LifecycleStatus} status.");
        }

        State = State with { LifecycleStatus = FacilityLifecycleStatus.Active, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityReopenedEvent(State.FacilityId, nowUtc));
    }

    /// <summary>Permanently decommissions a facility. Terminal -- no further lifecycle transitions are valid afterward.</summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">The facility is already <see cref="FacilityLifecycleStatus.Decommissioned"/>.</exception>
    public void DecommissionFacility(string reason, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Decommission reason is required.", nameof(reason));
        }
        if (State.LifecycleStatus == FacilityLifecycleStatus.Decommissioned)
        {
            throw new InvalidOperationException("Facility is already decommissioned.");
        }

        State = State with { LifecycleStatus = FacilityLifecycleStatus.Decommissioned, UpdatedAtUtc = nowUtc };
        _domainEvents.Add(new FacilityDecommissionedEvent(State.FacilityId, reason, nowUtc));
    }

    /// <summary>Clears every pending domain event, e.g. after a caller has persisted/published them.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
