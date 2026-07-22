// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           IMachineGuardingInspectionsRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the MachineGuardingInspections aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Interfaces;

/// <summary>Domain repository interface for the <see cref="MachineGuardingInspections"/> aggregate root.</summary>
public interface IMachineGuardingInspectionsRepository
{
    /// <summary>Retrieves a MachineGuardingInspections aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections?> GetByIdAsync(MachineGuardingInspectionsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all MachineGuardingInspections aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections>> ListAsync(MachineGuardingInspectionsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new MachineGuardingInspections aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing MachineGuardingInspections aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections aggregate, CancellationToken cancellationToken = default);
}