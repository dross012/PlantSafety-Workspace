// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Repositories
//  File:           IMachineRepository.cs
//  Created:        2026-08-08
//  Description:    Aggregate-level persistence for MachineAggregate. The
//                  in-memory implementation lives in
//                  Shared.MachineDomain.Infrastructure -- a real backing
//                  store is a future phase, not this one.
// ============================================================================
using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Repositories;

/// <summary>Aggregate-level persistence for <see cref="MachineAggregate"/>.</summary>
public interface IMachineRepository
{
    /// <summary>Finds the machine with the given id, or null if none exists.</summary>
    Task<MachineAggregate?> FindAsync(MachineId machineId, CancellationToken cancellationToken = default);

    /// <summary>Persists the aggregate's current state (create or overwrite).</summary>
    Task SaveAsync(MachineAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Returns every persisted machine, in no particular order.</summary>
    Task<IReadOnlyList<MachineAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}
