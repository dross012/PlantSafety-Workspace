// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         Persistence
//  File:           EmployeeStore.cs
//  Created:        2026-08-08
//  Description:    Thread-safe, in-memory, process-local store of
//                  EmployeeState keyed by EmployeeId. Backs EmployeeRepository.
//                  Mirrors Shared.FacilityDomain.Infrastructure's
//                  FacilityStore ConcurrentDictionary-backed store pattern.
// ============================================================================
using System.Collections.Concurrent;

using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Infrastructure.Persistence;

/// <summary>Thread-safe, in-memory access to persisted <see cref="EmployeeState"/> snapshots.</summary>
public interface IEmployeeStore
{
    /// <summary>Saves (creates or overwrites) the given state, keyed by its EmployeeId.</summary>
    void Save(EmployeeState state);

    /// <summary>Finds the state for the given employee id, or null if none exists.</summary>
    EmployeeState? Find(EmployeeId employeeId);

    /// <summary>Returns every persisted state, in no particular order.</summary>
    IReadOnlyList<EmployeeState> GetAll();
}

/// <summary>Thread-safe, in-memory, process-local <see cref="IEmployeeStore"/> implementation.</summary>
public sealed class EmployeeStore : IEmployeeStore
{
    private readonly ConcurrentDictionary<Guid, EmployeeState> _statesByEmployeeId = new();

    /// <inheritdoc />
    public void Save(EmployeeState state) => _statesByEmployeeId[state.EmployeeId.Value] = state;

    /// <inheritdoc />
    public EmployeeState? Find(EmployeeId employeeId) => _statesByEmployeeId.GetValueOrDefault(employeeId.Value);

    /// <inheritdoc />
    public IReadOnlyList<EmployeeState> GetAll() => _statesByEmployeeId.Values.ToArray();
}
