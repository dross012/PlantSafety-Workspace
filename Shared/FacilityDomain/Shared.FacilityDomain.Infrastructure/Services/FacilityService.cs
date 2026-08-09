// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Services
//  File:           FacilityService.cs
//  Created:        2026-08-08
//  Description:    IFacilityService implementation: loads (or creates) a
//                  FacilityAggregate, invokes the requested command,
//                  persists the result via IFacilityRepository, translates
//                  the aggregate's pending domain events into integration
//                  events, and publishes them through IModuleEventGateway
//                  before clearing them.
//
//                  Phase 4 (Facility domain pass), Section 2.3 "Handler
//                  Rules": previously used DateTime.UtcNow directly in
//                  every method (bypassing DeterministicMode entirely for
//                  the whole Facility domain) and called
//                  aggregate.ClearDomainEvents() immediately after save
//                  without ever publishing anything -- FacilityCreatedEvent
//                  was the only Facility event that ever reached a
//                  subscriber, and only because FacilitiesController
//                  separately constructed and published its own copy.
//                  Now uses IClock (Shared.Deterministic) for every
//                  timestamp and publishes a translated integration event
//                  (Events/FacilityIntegrationEvents.cs) for every domain
//                  event the aggregate actually raised, via
//                  PublishIsolatedAsync (Phase 3.4) so a failing subscriber
//                  can't turn an already-successful facility command into
//                  an error.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.FacilityDomain.Infrastructure.Events;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using DomainEvents = Shared.FacilityDomain.Abstractions.Events;

namespace Shared.FacilityDomain.Infrastructure.Services;

/// <summary><see cref="IFacilityService"/> implementation over <see cref="IFacilityRepository"/>.</summary>
public sealed class FacilityService(
    IFacilityRepository repository,
    IClock clock,
    IModuleEventGateway eventGateway,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext) : IFacilityService
{
    private const string SourceModule = "Facility";

    /// <inheritdoc />
    public async Task<FacilityState> CreateFacilityAsync(string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default)
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), name, industry, location, clock.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateFacilityMetadataAsync(FacilityId facilityId, string name, FacilityIndustry industry, FacilityLocation location, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateFacilityMetadata(name, industry, location, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateHazardProfileAsync(FacilityId facilityId, IReadOnlyList<HazardType> activeHazardTypes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateHazardProfile(activeHazardTypes, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateRegulatoryProfileAsync(FacilityId facilityId, IReadOnlyList<string> regulatorCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateRegulatoryProfile(regulatorCodes, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateProgramProfileAsync(FacilityId facilityId, IReadOnlyList<string> requiredProgramCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateProgramProfile(requiredProgramCodes, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddBuildingAsync(FacilityId facilityId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddBuilding(BuildingId.New(), name, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddAreaAsync(FacilityId facilityId, BuildingId buildingId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddArea(buildingId, AreaId.New(), name, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> AddZoneAsync(FacilityId facilityId, AreaId areaId, string name, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.AddZone(areaId, ZoneId.New(), name, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> UpdateHierarchyAsync(FacilityId facilityId, FacilityHierarchy hierarchy, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.UpdateHierarchy(hierarchy, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ActivateFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ActivateFacility(clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ShutdownFacilityAsync(FacilityId facilityId, string reason, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ShutdownFacility(reason, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> ReopenFacilityAsync(FacilityId facilityId, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.ReopenFacility(clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<FacilityState?> DecommissionFacilityAsync(FacilityId facilityId, string reason, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(facilityId, aggregate => aggregate.DecommissionFacility(reason, clock.UtcNow), cancellationToken);

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
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    // IModuleEventGateway.PublishAsync<TEvent> is generic, and the subscriber side keys off
    // typeof(TEvent) at the call site (Subscribe<FacilityCreatedEvent>, Subscribe<FacilityUpdatedEvent>,
    // etc.) -- publishing through a common `IModuleEvent?` return type would have the compiler
    // infer TEvent as the IModuleEvent interface, not the concrete event type, so nothing
    // subscribed to the concrete type would ever receive it (no exception, no log -- it just
    // silently goes to zero subscribers). Each domain event kind is dispatched to its own
    // concretely-typed PublishIsolatedAsync call instead, so generic inference picks up the
    // real type.
    private async Task PublishAndClearAsync(FacilityAggregate aggregate, CancellationToken cancellationToken)
    {
        foreach (object domainEvent in aggregate.DomainEvents)
        {
            switch (domainEvent)
            {
                case DomainEvents.FacilityCreatedEvent e:
                    await PublishAsync(new FacilityCreatedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, e.Name), cancellationToken);
                    break;
                case DomainEvents.FacilityUpdatedEvent e:
                    await PublishAsync(new FacilityUpdatedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, e.UpdateKind.ToString()), cancellationToken);
                    break;
                case DomainEvents.FacilityHierarchyUpdatedEvent e:
                    await PublishAsync(new FacilityHierarchyUpdatedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, e.Kind.ToString()), cancellationToken);
                    break;
                case DomainEvents.FacilityActivatedEvent e:
                    await PublishAsync(new FacilityLifecycleChangedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, "Active", null), cancellationToken);
                    break;
                case DomainEvents.FacilityShutdownEvent e:
                    await PublishAsync(new FacilityLifecycleChangedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, "ShutDown", e.Reason), cancellationToken);
                    break;
                case DomainEvents.FacilityReopenedEvent e:
                    await PublishAsync(new FacilityLifecycleChangedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, "Active", null), cancellationToken);
                    break;
                case DomainEvents.FacilityDecommissionedEvent e:
                    await PublishAsync(new FacilityLifecycleChangedEvent(SourceModule, e.OccurredAtUtc, e.FacilityId.Value, "Decommissioned", e.Reason), cancellationToken);
                    break;
            }
        }

        aggregate.ClearDomainEvents();
    }

    private Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) where TEvent : IModuleEvent =>
        eventGateway.PublishIsolatedAsync(integrationEvent, logEventWriter, correlationContext.CorrelationId, cancellationToken);
}
