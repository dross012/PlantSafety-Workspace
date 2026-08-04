// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           ITsdfFacilityRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the TsdfFacility aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Interfaces;

/// <summary>Domain repository interface for the TsdfFacility aggregate root.</summary>
public interface ITsdfFacilityRepository
{
    /// <summary>Retrieves a TsdfFacility aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility?> GetByIdAsync(TsdfFacilityId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all TsdfFacility aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility>> ListAsync(TsdfFacilitySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new TsdfFacility aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing TsdfFacility aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility aggregate, CancellationToken cancellationToken = default);
}
