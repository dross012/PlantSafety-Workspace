// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           IPPETrainingFitRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPETrainingFit aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPETrainingFit"/> aggregate root.</summary>
public interface IPPETrainingFitRepository
{
    /// <summary>Retrieves a PPETrainingFit aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit?> GetByIdAsync(PPETrainingFitId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPETrainingFit aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit>> ListAsync(PPETrainingFitSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPETrainingFit aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPETrainingFit aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit aggregate, CancellationToken cancellationToken = default);
}