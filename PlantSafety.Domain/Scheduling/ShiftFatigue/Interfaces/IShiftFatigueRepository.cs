// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           IShiftFatigueRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ShiftFatigue aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces;

/// <summary>Domain repository interface for the <see cref="ShiftFatigue"/> aggregate root.</summary>
public interface IShiftFatigueRepository
{
    /// <summary>Retrieves a ShiftFatigue aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue?> GetByIdAsync(ShiftFatigueId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ShiftFatigue aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue>> ListAsync(ShiftFatigueSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ShiftFatigue aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ShiftFatigue aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue aggregate, CancellationToken cancellationToken = default);
}