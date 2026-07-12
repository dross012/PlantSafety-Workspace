// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           IDigitalTwinRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DigitalTwin aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Repositories;

/// <summary>Domain repository interface for the <see cref="DigitalTwin"/> aggregate root.</summary>
public interface IDigitalTwinRepository
{
    /// <summary>Retrieves a DigitalTwin aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin?> GetByIdAsync(DigitalTwinId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DigitalTwin aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DigitalTwin aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin aggregate, CancellationToken cancellationToken = default);
}
