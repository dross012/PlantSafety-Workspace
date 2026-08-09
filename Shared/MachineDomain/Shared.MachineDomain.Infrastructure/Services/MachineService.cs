// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         Services
//  File:           MachineService.cs
//  Created:        2026-08-08
//  Description:    IMachineService implementation: loads (or creates) a
//                  MachineAggregate, invokes the requested command,
//                  persists the result via IMachineRepository, and clears
//                  the aggregate's pending domain events -- nothing
//                  publishes them yet, mirroring Shared.FacilityDomain's
//                  FacilityService / Shared.EmployeeDomain's EmployeeService
//                  precedent.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Infrastructure.Services;

/// <summary><see cref="IMachineService"/> implementation over <see cref="IMachineRepository"/>.</summary>
public sealed class MachineService(IMachineRepository repository) : IMachineService
{
    /// <inheritdoc />
    public async Task<MachineState> CreateMachineAsync(string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default)
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), name, machineType, location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<MachineState?> UpdateMachineMetadataAsync(MachineId machineId, string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.UpdateMachineMetadata(name, machineType, location, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<MachineState?> UpdateMaintenanceProfileAsync(MachineId machineId, DateTime? lastMaintenanceAtUtc, DateTime? nextScheduledMaintenanceAtUtc, int maintenanceIntervalDays, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.UpdateMaintenanceProfile(lastMaintenanceAtUtc, nextScheduledMaintenanceAtUtc, maintenanceIntervalDays, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<MachineState?> RecordFailureAsync(MachineId machineId, string description, HazardType resultingHazardType, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(machineId, aggregate => aggregate.RecordFailure(description, resultingHazardType, DateTime.UtcNow), cancellationToken);

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
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }
}
