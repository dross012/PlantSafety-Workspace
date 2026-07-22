// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           ISafetyValvesRelaysRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SafetyValvesRelays aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces;

/// <summary>Domain repository interface for the <see cref="SafetyValvesRelays"/> aggregate root.</summary>
public interface ISafetyValvesRelaysRepository
{
    /// <summary>Retrieves a SafetyValvesRelays aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays?> GetByIdAsync(SafetyValvesRelaysId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all SafetyValvesRelays aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays>> ListAsync(SafetyValvesRelaysSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SafetyValvesRelays aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SafetyValvesRelays aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays aggregate, CancellationToken cancellationToken = default);
}