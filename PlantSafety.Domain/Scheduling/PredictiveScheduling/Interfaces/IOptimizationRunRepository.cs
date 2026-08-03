// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           IOptimizationRunRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the OptimizationRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Specifications;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

/// <summary>Domain repository interface for the OptimizationRun aggregate root.</summary>
public interface IOptimizationRunRepository
{
    /// <summary>
    /// Retrieves an OptimizationRun aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the run belongs to a different tenant.
    /// </summary>
    Task<OptimizationRun?> GetByIdAsync(OptimizationRunId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all OptimizationRun aggregates for the given tenant matching the specification.</summary>
    Task<IReadOnlyList<OptimizationRun>> ListAsync(Guid tenantId, OptimizationRunSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new OptimizationRun aggregate (Pending status, no results yet).</summary>
    Task AddAsync(OptimizationRun aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing OptimizationRun aggregate (status/result transitions).</summary>
    Task UpdateAsync(OptimizationRun aggregate, CancellationToken cancellationToken = default);
}
