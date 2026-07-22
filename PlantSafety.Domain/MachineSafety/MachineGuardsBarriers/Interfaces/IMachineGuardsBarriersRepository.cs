// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           IMachineGuardsBarriersRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the MachineGuardsBarriers aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces;

/// <summary>Domain repository interface for the <see cref="MachineGuardsBarriers"/> aggregate root.</summary>
public interface IMachineGuardsBarriersRepository
{
    /// <summary>Retrieves a MachineGuardsBarriers aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers?> GetByIdAsync(MachineGuardsBarriersId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all MachineGuardsBarriers aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers>> ListAsync(MachineGuardsBarriersSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new MachineGuardsBarriers aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing MachineGuardsBarriers aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers aggregate, CancellationToken cancellationToken = default);
}