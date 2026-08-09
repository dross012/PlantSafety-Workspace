// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Repositories
//  File:           IEmployeeRepository.cs
//  Created:        2026-08-08
//  Description:    Aggregate-level persistence for EmployeeAggregate. The
//                  in-memory implementation lives in
//                  Shared.EmployeeDomain.Infrastructure -- a real backing
//                  store is a future phase, not this one.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Repositories;

/// <summary>Aggregate-level persistence for <see cref="EmployeeAggregate"/>.</summary>
public interface IEmployeeRepository
{
    /// <summary>Finds the employee with the given id, or null if none exists.</summary>
    Task<EmployeeAggregate?> FindAsync(EmployeeId employeeId, CancellationToken cancellationToken = default);

    /// <summary>Persists the aggregate's current state (create or overwrite).</summary>
    Task SaveAsync(EmployeeAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Returns every persisted employee, in no particular order.</summary>
    Task<IReadOnlyList<EmployeeAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}
