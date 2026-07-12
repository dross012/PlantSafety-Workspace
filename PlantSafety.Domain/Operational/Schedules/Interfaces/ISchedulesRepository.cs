// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           ISchedulesRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Schedules aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Interfaces;

/// <summary>Domain repository interface for the <see cref="Schedules"/> aggregate root.</summary>
public interface ISchedulesRepository
{
    /// <summary>Retrieves a Schedules aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules?> GetByIdAsync(SchedulesId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Schedules aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules>> ListAsync(SchedulesSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Schedules aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Schedules aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules aggregate, CancellationToken cancellationToken = default);
}
