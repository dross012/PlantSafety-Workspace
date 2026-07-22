// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           ICultureAssessmentRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the CultureAssessment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;

/// <summary>Domain repository interface for the <see cref="CultureAssessment"/> aggregate root.</summary>
public interface ICultureAssessmentRepository
{
    /// <summary>Retrieves a CultureAssessment aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment?> GetByIdAsync(CultureAssessmentId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all CultureAssessment aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment>> ListAsync(CultureAssessmentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new CultureAssessment aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CultureAssessment aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment aggregate, CancellationToken cancellationToken = default);
}