// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           IMachineGuardingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the MachineGuarding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Interfaces;

/// <summary>Domain repository interface for the <see cref="MachineGuarding"/> aggregate root.</summary>
public interface IMachineGuardingRepository
{
    /// <summary>Retrieves a MachineGuarding aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding?> GetByIdAsync(MachineGuardingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all MachineGuarding aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding>> ListAsync(MachineGuardingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new MachineGuarding aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing MachineGuarding aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding aggregate, CancellationToken cancellationToken = default);
}