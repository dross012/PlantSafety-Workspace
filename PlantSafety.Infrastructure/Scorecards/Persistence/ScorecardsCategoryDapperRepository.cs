// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Infrastructure
//  File:           ScorecardsCategoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Single Dapper repository implementing all 6 Scorecards category read-model
//                  Application-layer repository interfaces. LaggingIndicators, LeadingIndicators,
//                  and CulturalPrecursorMetrics map 1:1 onto the Scorecard aggregate's own
//                  IndicatorCategory taxonomy and filter by a literal @Category value.
//                  ReportingDistribution, ScorecardDataSources, and
//                  ScorecardStructureVisualization are cross-cutting scorecard facets with no
//                  corresponding IndicatorCategory value -- ASSUMPTION (flagged): for those three,
//                  this repository passes @Category = NULL to the shared procedure (returning all
//                  scorecards for the tenant/site/date range, unfiltered by category) and stamps
//                  the DTO's Category field with the read-model's own folder name for UI/reporting
//                  consistency, since the Scorecard aggregate has no other field to distinguish
//                  these three facets.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Models;
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Repositories;
using PlantSafety.Application.Scorecards.LaggingIndicators.Models;
using PlantSafety.Application.Scorecards.LaggingIndicators.Repositories;
using PlantSafety.Application.Scorecards.LeadingIndicators.Models;
using PlantSafety.Application.Scorecards.LeadingIndicators.Repositories;
using PlantSafety.Application.Scorecards.ReportingDistribution.Models;
using PlantSafety.Application.Scorecards.ReportingDistribution.Repositories;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Models;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Repositories;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Models;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Repositories;

namespace PlantSafety.Infrastructure.Scorecards.Persistence;

/// <summary>
/// Dapper-backed repository implementing all 6 Scorecards category read-model repository
/// interfaces over the shared <c>[Scorecards].[usp_Scorecards_GetByCategory]</c> procedure.
/// </summary>
public sealed class ScorecardsCategoryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ScorecardsCategoryDapperRepository> logger) :
    ILaggingIndicatorsRepository,
    ILeadingIndicatorsRepository,
    ICulturalPrecursorMetricsRepository,
    IReportingDistributionRepository,
    IScorecardDataSourcesRepository,
    IScorecardStructureVisualizationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LaggingIndicatorsItemDto>> GetLaggingIndicatorsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, "Lagging", cancellationToken);
        return rows.Select(r => new LaggingIndicatorsItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = r.Category, Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LeadingIndicatorsItemDto>> GetLeadingIndicatorsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, "Leading", cancellationToken);
        return rows.Select(r => new LeadingIndicatorsItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = r.Category, Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CulturalPrecursorMetricsItemDto>> GetCulturalPrecursorMetricsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, "Cultural", cancellationToken);
        return rows.Select(r => new CulturalPrecursorMetricsItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = r.Category, Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReportingDistributionItemDto>> GetReportingDistributionAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, category: null, cancellationToken);
        return rows.Select(r => new ReportingDistributionItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = "ReportingDistribution", Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScorecardDataSourcesItemDto>> GetScorecardDataSourcesAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, category: null, cancellationToken);
        return rows.Select(r => new ScorecardDataSourcesItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = "ScorecardDataSources", Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScorecardStructureVisualizationItemDto>> GetScorecardStructureVisualizationAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(tenantId, siteId, from, to, category: null, cancellationToken);
        return rows.Select(r => new ScorecardStructureVisualizationItemDto
        {
            ScorecardId = r.ScorecardId, TenantId = r.TenantId, SiteId = r.SiteId, Title = r.Title,
            Category = "ScorecardStructureVisualization", Status = r.Status, EffectiveAtUtc = r.EffectiveAtUtc, OwnerId = r.OwnerId,
        }).ToList();
    }

    private async Task<IReadOnlyList<CategoryRow>> QueryAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, string? category, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Scorecards category {Category} for tenant {TenantId}", category ?? "(all)", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", category, 30)
            .Build();

        return await queryExecutor.QueryAsync<CategoryRow>(
            "[Scorecards].[usp_Scorecards_GetByCategory]", parameters, cancellationToken);
    }

    /// <summary>Flat row shape returned by <c>usp_Scorecards_GetByCategory</c>.</summary>
    private sealed record CategoryRow
    {
        public required Guid ScorecardId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Title { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
    }
}
