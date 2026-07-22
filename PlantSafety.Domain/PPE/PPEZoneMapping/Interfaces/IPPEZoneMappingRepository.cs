// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           IPPEZoneMappingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPEZoneMapping aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPEZoneMapping"/> aggregate root.</summary>
public interface IPPEZoneMappingRepository
{
    /// <summary>Retrieves a PPEZoneMapping aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping?> GetByIdAsync(PPEZoneMappingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPEZoneMapping aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping>> ListAsync(PPEZoneMappingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPEZoneMapping aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPEZoneMapping aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping aggregate, CancellationToken cancellationToken = default);
}