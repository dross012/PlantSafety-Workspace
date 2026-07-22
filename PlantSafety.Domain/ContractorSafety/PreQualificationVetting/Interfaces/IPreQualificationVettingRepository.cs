// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           IPreQualificationVettingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PreQualificationVetting aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Interfaces;

/// <summary>Domain repository interface for the <see cref="PreQualificationVetting"/> aggregate root.</summary>
public interface IPreQualificationVettingRepository
{
    /// <summary>Retrieves a PreQualificationVetting aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting?> GetByIdAsync(PreQualificationVettingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PreQualificationVetting aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting>> ListAsync(PreQualificationVettingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PreQualificationVetting aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PreQualificationVetting aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting aggregate, CancellationToken cancellationToken = default);
}