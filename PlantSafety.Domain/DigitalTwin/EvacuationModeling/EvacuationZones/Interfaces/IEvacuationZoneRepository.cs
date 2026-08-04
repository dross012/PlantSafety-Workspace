// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           IEvacuationZoneRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EvacuationZone aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.EvacuationZone"/> aggregate root.</summary>
public interface IEvacuationZoneRepository
{
    /// <summary>Retrieves an EvacuationZone aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone?> GetByIdAsync(EvacuationZoneId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EvacuationZone aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone>> ListAsync(EvacuationZoneSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EvacuationZone aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EvacuationZone aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone aggregate, CancellationToken cancellationToken = default);
}
