// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           IDashboardReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Dashboard root-aggregate read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Analytics.Dashboard.Interfaces.IDashboardRepository (the
//                  write-side aggregate repository) and from the pre-existing sub-feature
//                  repositories (Executive/KPISnapshots/ReadModel/TrendAnalysis) -- same kind of
//                  CQRS read/write separation as
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.Repositories;

/// <summary>Read-only repository for Dashboard root-aggregate data. Implemented in the Infrastructure layer.</summary>
public interface IDashboardReadRepository
{
    /// <summary>
    /// Returns the Dashboard read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the Dashboard record belongs to a different tenant.
    /// </summary>
    Task<DashboardItemDto?> GetByIdAsync(Guid dashboardId, Guid tenantId, CancellationToken cancellationToken);
}
