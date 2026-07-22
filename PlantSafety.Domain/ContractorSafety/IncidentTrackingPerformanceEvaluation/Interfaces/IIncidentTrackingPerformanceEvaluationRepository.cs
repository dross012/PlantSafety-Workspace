// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IIncidentTrackingPerformanceEvaluationRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the IncidentTrackingPerformanceEvaluation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Interfaces;

/// <summary>Domain repository interface for the <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate root.</summary>
public interface IIncidentTrackingPerformanceEvaluationRepository
{
    /// <summary>Retrieves a IncidentTrackingPerformanceEvaluation aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation?> GetByIdAsync(IncidentTrackingPerformanceEvaluationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all IncidentTrackingPerformanceEvaluation aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation>> ListAsync(IncidentTrackingPerformanceEvaluationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new IncidentTrackingPerformanceEvaluation aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing IncidentTrackingPerformanceEvaluation aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation aggregate, CancellationToken cancellationToken = default);
}