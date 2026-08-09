// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         Persistence
//  File:           MachineRepository.cs
//  Created:        2026-08-08
//  Description:    In-memory IMachineRepository implementation, backed by
//                  IMachineStore. Rehydrates a fresh MachineAggregate on
//                  every FindAsync call (no aggregate-instance caching), so
//                  each caller gets its own DomainEvents list.
// ============================================================================
using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Infrastructure.Persistence;

/// <summary>In-memory <see cref="IMachineRepository"/> implementation, backed by <see cref="IMachineStore"/>.</summary>
public sealed class MachineRepository(IMachineStore store) : IMachineRepository
{
    /// <inheritdoc />
    public Task<MachineAggregate?> FindAsync(MachineId machineId, CancellationToken cancellationToken = default)
    {
        MachineState? state = store.Find(machineId);
        return Task.FromResult(state is null ? null : MachineAggregate.Rehydrate(state));
    }

    /// <inheritdoc />
    public Task SaveAsync(MachineAggregate aggregate, CancellationToken cancellationToken = default)
    {
        store.Save(aggregate.State);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MachineAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MachineAggregate> aggregates = store.GetAll().Select(MachineAggregate.Rehydrate).ToArray();
        return Task.FromResult(aggregates);
    }
}
