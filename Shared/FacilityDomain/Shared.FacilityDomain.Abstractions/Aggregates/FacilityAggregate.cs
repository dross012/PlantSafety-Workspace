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

    /// <summary>Clears every pending domain event, e.g. after a caller has persisted/published them.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
