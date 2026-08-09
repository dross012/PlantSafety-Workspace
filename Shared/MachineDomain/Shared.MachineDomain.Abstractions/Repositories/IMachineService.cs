// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Repositories
//  File:           IMachineService.cs
//  Created:        2026-08-08
//  Description:    The outward-facing application service over
//                  MachineAggregate/IMachineRepository -- callers outside
//                  Shared.MachineDomain depend on this, not on the
//                  aggregate or repository directly.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Repositories;

/// <summary>Application service orchestrating MachineAggregate commands against IMachineRepository.</summary>
public interface IMachineService
{
    /// <summary>Creates a new machine and returns its resulting state.</summary>
    Task<MachineState> CreateMachineAsync(string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing machine's metadata. Returns null if no machine with that id exists.</summary>
    Task<MachineState?> UpdateMachineMetadataAsync(MachineId machineId, string name, MachineType machineType, MachineLocation location, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing machine's maintenance profile. Returns null if no machine with that id exists.</summary>
    Task<MachineState?> UpdateMaintenanceProfileAsync(MachineId machineId, DateTime? lastMaintenanceAtUtc, DateTime? nextScheduledMaintenanceAtUtc, int maintenanceIntervalDays, CancellationToken cancellationToken = default);

    /// <summary>Records a failure for an existing machine. Returns null if no machine with that id exists.</summary>
    Task<MachineState?> RecordFailureAsync(MachineId machineId, string description, HazardType resultingHazardType, CancellationToken cancellationToken = default);

    /// <summary>Returns the machine with the given id, or null if none exists.</summary>
    Task<MachineState?> GetMachineAsync(MachineId machineId, CancellationToken cancellationToken = default);

    /// <summary>Returns every machine, in no particular order.</summary>
    Task<IReadOnlyList<MachineState>> GetAllMachinesAsync(CancellationToken cancellationToken = default);
}
