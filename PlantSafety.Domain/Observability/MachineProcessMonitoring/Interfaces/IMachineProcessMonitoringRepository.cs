// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           IMachineProcessMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the MachineProcessMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces;

/// <summary>Domain repository interface for the <see cref="MachineProcessMonitoring"/> aggregate root.</summary>
public interface IMachineProcessMonitoringRepository
{
    /// <summary>Retrieves a MachineProcessMonitoring aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring?> GetByIdAsync(MachineProcessMonitoringId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all MachineProcessMonitoring aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring>> ListAsync(MachineProcessMonitoringSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new MachineProcessMonitoring aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing MachineProcessMonitoring aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring aggregate, CancellationToken cancellationToken = default);
}