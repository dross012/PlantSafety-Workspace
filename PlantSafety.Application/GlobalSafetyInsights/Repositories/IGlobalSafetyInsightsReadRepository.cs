// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           IGlobalSafetyInsightsReadRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the GlobalSafetyInsights read-model.
//                  Implemented in the Infrastructure layer. Mirrors
//                  Analytics.Dashboard.Executive.Repositories.IExecutiveRepository's convention.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Repositories;

/// <summary>Read-only repository for GlobalSafetyInsights data. Implemented in the Infrastructure layer.</summary>
public interface IGlobalSafetyInsightsReadRepository
{
    /// <summary>Returns the most recently computed snapshot for the given tenant, or <see langword="null"/> if none exists.</summary>
    Task<GlobalSafetyInsightsItemDto?> GetLatestAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns every snapshot for the given tenant within the given UTC date range, most recent first.</summary>
    Task<IReadOnlyList<GlobalSafetyInsightsItemDto>> GetHistoryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
