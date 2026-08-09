// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         Persistence
//  File:           MachineStore.cs
//  Created:        2026-08-08
//  Description:    Thread-safe, in-memory, process-local store of
//                  MachineState keyed by MachineId. Backs MachineRepository.
//                  Mirrors Shared.FacilityDomain.Infrastructure's
//                  FacilityStore / Shared.EmployeeDomain.Infrastructure's
//                  EmployeeStore ConcurrentDictionary-backed store pattern.
// ============================================================================
using System.Collections.Concurrent;

using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Infrastructure.Persistence;

/// <summary>Thread-safe, in-memory access to persisted <see cref="MachineState"/> snapshots.</summary>
public interface IMachineStore
{
    /// <summary>Saves (creates or overwrites) the given state, keyed by its MachineId.</summary>
    void Save(MachineState state);

    /// <summary>Finds the state for the given machine id, or null if none exists.</summary>
    MachineState? Find(MachineId machineId);

    /// <summary>Returns every persisted state, in no particular order.</summary>
    IReadOnlyList<MachineState> GetAll();
}

/// <summary>Thread-safe, in-memory, process-local <see cref="IMachineStore"/> implementation.</summary>
public sealed class MachineStore : IMachineStore
{
    private readonly ConcurrentDictionary<Guid, MachineState> _statesByMachineId = new();

    /// <inheritdoc />
    public void Save(MachineState state) => _statesByMachineId[state.MachineId.Value] = state;

    /// <inheritdoc />
    public MachineState? Find(MachineId machineId) => _statesByMachineId.GetValueOrDefault(machineId.Value);

    /// <inheritdoc />
    public IReadOnlyList<MachineState> GetAll() => _statesByMachineId.Values.ToArray();
}
