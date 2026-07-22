// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           IStopWorkEventRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the StopWorkEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Interfaces;

/// <summary>Domain repository interface for the <see cref="StopWorkEvent"/> aggregate root.</summary>
public interface IStopWorkEventRepository
{
    /// <summary>Retrieves a StopWorkEvent aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent?> GetByIdAsync(StopWorkEventId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all StopWorkEvent aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent>> ListAsync(StopWorkEventSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new StopWorkEvent aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing StopWorkEvent aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent aggregate, CancellationToken cancellationToken = default);
}