// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         Services
//  File:           MachineService.cs
//  Created:        2026-08-08
//  Description:    IMachineService implementation: loads (or creates) a
//                  MachineAggregate, invokes the requested command,
//                  persists the result via IMachineRepository, translates
//                  the aggregate's pending domain events into integration
//                  events, and publishes them through IModuleEventGateway
//                  before clearing them.
//
//                  Phase 4 (Machine domain pass), mirroring the Facility
//                  and Employee domain passes' fix: previously used
//                  DateTime.UtcNow directly in every method (bypassing
//                  DeterministicMode for the whole Machine domain) and
//                  called aggregate.ClearDomainEvents() immediately after
//                  save without ever publishing anything -- every Machine
//                  domain event (Created/Updated/MaintenanceUpdated/
//                  FailureRecorded) was silently discarded, including
//                  MachineFailureRecordedEvent, whose own header comment
//                  says it exists so "downstream hazard propagation...
//                  can react without re-deriving it" -- a consumer that
//                  could never have worked, since nothing ever published
//                  the event it would subscribe to. Now uses IClock for
//                  every timestamp and publishes a translated integration
//                  event (Events/MachineIntegrationEvents.cs) for every
//                  domain event the aggregate actually raised, via
//                  PublishIsolatedAsync (Phase 3.4).
//
//                  Each domain-event case is dispatched to its own
//                  concretely-typed PublishAsync<TEvent> call rather than
//                  translated through a common IModuleEvent?-typed helper
//                  -- the Facility domain pass found that erasing through
//                  a common interface-typed return value makes C# infer
//                  the generic PublishAsync<TEvent> call's TEvent as the
//                  interface, not the concrete type, so nothing subscribed
//                  to the concrete type would ever receive it.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Infrastructure.Events;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using DomainEvents = Shared.MachineDomain.Abstractions.Events;

namespace Shared.MachineDomain.Infrastructure.Services;

/// <summary><see cref="IMachineService"/> implementation over <see cref="IMachineRepository"/>.</summary>
public sealed class MachineService(
    IMachineRepository repository,
    IClock clock,
    IModuleEventGateway eventGateway,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext) : IMachineService
{
    private const string SourceModule = "Machine";

    /// <inheritdoc />
    public async Task<MachineState> CreateMachineAsync(string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default)
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), name, machineType, location, clock.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<MachineState?> UpdateMachineMetadataAsync(MachineId machineId, string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.UpdateMachineMetadata(name, machineType, location, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<MachineState?> UpdateMaintenanceProfileAsync(MachineId machineId, DateTime? lastMaintenanceAtUtc, DateTime? nextScheduledMaintenanceAtUtc, int maintenanceIntervalDays, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.UpdateMaintenanceProfile(lastMaintenanceAtUtc, nextScheduledMaintenanceAtUtc, maintenanceIntervalDays, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<MachineState?> RecordFailureAsync(MachineId machineId, string description, HazardType resultingHazardType, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.RecordFailure(description, resultingHazardType, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<MachineState?> GetMachineAsync(MachineId machineId, CancellationToken cancellationToken = default)
    {
        MachineAggregate? aggregate = await repository.FindAsync(machineId, cancellationToken);
        return aggregate?.State;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MachineState>> GetAllMachinesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MachineAggregate> aggregates = await repository.GetAllAsync(cancellationToken);
        return aggregates.Select(aggregate => aggregate.State).ToArray();
    }

    private async Task<MachineState?> ApplyUpdateAsync(MachineId machineId, Action<MachineAggregate> applyCommand, CancellationToken cancellationToken)
    {
        MachineAggregate? aggregate = await repository.FindAsync(machineId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        applyCommand(aggregate);
        await repository.SaveAsync(aggregate, cancellationToken);
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    private async Task PublishAndClearAsync(MachineAggregate aggregate, CancellationToken cancellationToken)
    {
        foreach (object domainEvent in aggregate.DomainEvents)
        {
            switch (domainEvent)
            {
                case DomainEvents.MachineCreatedEvent e:
                    await PublishAsync(new MachineCreatedEvent(SourceModule, e.OccurredAtUtc, e.MachineId.Value, e.Name), cancellationToken);
                    break;
                case DomainEvents.MachineUpdatedEvent e:
                    await PublishAsync(new MachineUpdatedEvent(SourceModule, e.OccurredAtUtc, e.MachineId.Value, e.Kind.ToString()), cancellationToken);
                    break;
                case DomainEvents.MachineMaintenanceUpdatedEvent e:
                    await PublishAsync(new MachineMaintenanceUpdatedEvent(SourceModule, e.OccurredAtUtc, e.MachineId.Value, e.NextScheduledMaintenanceAtUtc), cancellationToken);
                    break;
                case DomainEvents.MachineFailureRecordedEvent e:
                    await PublishAsync(new MachineFailureRecordedEvent(SourceModule, e.OccurredAtUtc, e.MachineId.Value, e.Description, e.ResultingHazardType), cancellationToken);
                    break;
            }
        }

        aggregate.ClearDomainEvents();
    }

    private Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) where TEvent : IModuleEvent =>
        eventGateway.PublishIsolatedAsync(integrationEvent, logEventWriter, correlationContext.CorrelationId, cancellationToken);
}
