// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         Persistence
//  File:           EmployeeRepository.cs
//  Created:        2026-08-08
//  Description:    In-memory IEmployeeRepository implementation, backed by
//                  IEmployeeStore. Rehydrates a fresh EmployeeAggregate on
//                  every FindAsync call (no aggregate-instance caching), so
//                  each caller gets its own DomainEvents list.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Infrastructure.Persistence;

/// <summary>In-memory <see cref="IEmployeeRepository"/> implementation, backed by <see cref="IEmployeeStore"/>.</summary>
public sealed class EmployeeRepository(IEmployeeStore store) : IEmployeeRepository
{
    /// <inheritdoc />
    public Task<EmployeeAggregate?> FindAsync(EmployeeId employeeId, CancellationToken cancellationToken = default)
    {
        EmployeeState? state = store.Find(employeeId);
        return Task.FromResult(state is null ? null : EmployeeAggregate.Rehydrate(state));
    }

    /// <inheritdoc />
    public Task SaveAsync(EmployeeAggregate aggregate, CancellationToken cancellationToken = default)
    {
        store.Save(aggregate.State);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EmployeeAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmployeeAggregate> aggregates = store.GetAll().Select(EmployeeAggregate.Rehydrate).ToArray();
        return Task.FromResult(aggregates);
    }
}
