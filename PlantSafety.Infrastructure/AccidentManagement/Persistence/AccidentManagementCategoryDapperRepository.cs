// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Infrastructure
//  File:           AccidentManagementCategoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Single Dapper repository implementing all 4 AccidentManagement category
//                  read-model Application-layer repository interfaces (one per InvestigationPhase
//                  taxonomy entry). Each method calls the single shared
//                  [AccidentManagement].[usp_Accidents_GetByCategory] stored procedure with its
//                  own literal @Category value, avoiding 4 near-duplicate query shapes.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Models;
using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Repositories;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Models;
using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Repositories;
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Models;
using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Repositories;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Models;
using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Repositories;

namespace PlantSafety.Infrastructure.AccidentManagement.Persistence;

/// <summary>
/// Dapper-backed repository implementing all 4 AccidentManagement category read-model repository
/// interfaces over the shared <c>[AccidentManagement].[usp_Accidents_GetByCategory]</c> procedure.
/// </summary>
public sealed class AccidentManagementCategoryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AccidentManagementCategoryDapperRepository> logger) :
    ISiteSecuringAndTriageRepository,
    IInvestigationRootCauseAnalysisRepository,
    ICorrectiveActionsCAPARepository,
    IRegulatoryComplianceDocumentationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SiteSecuringAndTriageItemDto>> GetSiteSecuringAndTriageAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSiteSecuringAndTriage for AccidentManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "SiteSecuringAndTriage", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[AccidentManagement].[usp_Accidents_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new SiteSecuringAndTriageItemDto
        {
            AccidentId = r.AccidentId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<InvestigationRootCauseAnalysisItemDto>> GetInvestigationRootCauseAnalysisAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetInvestigationRootCauseAnalysis for AccidentManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "InvestigationRootCauseAnalysis", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[AccidentManagement].[usp_Accidents_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new InvestigationRootCauseAnalysisItemDto
        {
            AccidentId = r.AccidentId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CorrectiveActionsCAPAItemDto>> GetCorrectiveActionsCAPAAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCorrectiveActionsCAPA for AccidentManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "CorrectiveActionsCAPA", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[AccidentManagement].[usp_Accidents_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new CorrectiveActionsCAPAItemDto
        {
            AccidentId = r.AccidentId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegulatoryComplianceDocumentationItemDto>> GetRegulatoryComplianceDocumentationAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRegulatoryComplianceDocumentation for AccidentManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "RegulatoryComplianceDocumentation", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[AccidentManagement].[usp_Accidents_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new RegulatoryComplianceDocumentationItemDto
        {
            AccidentId = r.AccidentId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <summary>Flat row shape returned by <c>usp_Accidents_GetByCategory</c>.</summary>
    private sealed record CategoryRow
    {
        public required Guid AccidentId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Title { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
    }
}
