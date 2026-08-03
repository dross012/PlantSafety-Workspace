// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Infrastructure
//  File:           UnifiedModelDashboardDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Single Dapper repository implementing all six unified-analytics-model BI
//                  dashboard read-model repository interfaces (Scheduling, Production,
//                  Maintenance, Risk, ContinuousImprovement, Audit). Each method calls its own
//                  domain-specific stored procedure over the corresponding fact table under the
//                  [AnalyticsUnifiedModel] SQL schema. Mirrors the established multi-interface,
//                  single-class pattern used by RiskManagementCategoryDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Repositories;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Repositories;

namespace PlantSafety.Infrastructure.Analytics.UnifiedModel.Persistence;

/// <summary>
/// Dapper-backed repository implementing all six unified-analytics-model BI dashboard
/// read-model repository interfaces (Rule: Phase 5 §4.3), each backed by its own
/// <c>[AnalyticsUnifiedModel].[usp_&lt;Domain&gt;Dashboard_GetDaily]</c> stored procedure.
/// </summary>
public sealed class UnifiedModelDashboardDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<UnifiedModelDashboardDapperRepository> logger) :
    ISchedulingDashboardRepository,
    IProductionDashboardRepository,
    IMaintenanceDashboardRepository,
    IRiskDashboardRepository,
    IContinuousImprovementDashboardRepository,
    IAuditDashboardRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SchedulingDashboardItemDto>> GetSchedulingDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSchedulingDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SchedulingDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_SchedulingDashboard_GetDaily]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductionDashboardItemDto>> GetProductionDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetProductionDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ProductionDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_ProductionDashboard_GetDaily]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MaintenanceDashboardItemDto>> GetMaintenanceDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMaintenanceDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MaintenanceDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_MaintenanceDashboard_GetDaily]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RiskDashboardItemDto>> GetRiskDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRiskDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RiskDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_RiskDashboard_GetDaily]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContinuousImprovementDashboardItemDto>> GetContinuousImprovementDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContinuousImprovementDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ContinuousImprovementDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_ContinuousImprovementDashboard_GetDaily]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditDashboardItemDto>> GetAuditDashboardAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAuditDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AuditDashboardItemDto>(
            "[AnalyticsUnifiedModel].[usp_AuditDashboard_GetDaily]", parameters, cancellationToken);
    }
}
