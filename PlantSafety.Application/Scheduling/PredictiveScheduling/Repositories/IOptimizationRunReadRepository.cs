// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           IOptimizationRunReadRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the OptimizationRun read-model. Implemented
//                  in the Infrastructure layer. Distinct simple name from the Domain-layer
//                  (write-side) IOptimizationRunRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Repositories;

/// <summary>Read-only repository for OptimizationRun read-model data. Implemented in the Infrastructure layer.</summary>
public interface IOptimizationRunReadRepository
{
    /// <summary>
    /// Returns the OptimizationRun read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<OptimizationRunDto?> GetByIdAsync(Guid optimizationRunId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns OptimizationRun summary records for the given tenant, optionally scoped to a site,
    /// filtered to the given inclusive UTC request date range and/or status code, most-recent first.
    /// </summary>
    Task<IReadOnlyList<OptimizationRunSummaryDto>> ListHistoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        string? statusCode,
        CancellationToken cancellationToken);
}
