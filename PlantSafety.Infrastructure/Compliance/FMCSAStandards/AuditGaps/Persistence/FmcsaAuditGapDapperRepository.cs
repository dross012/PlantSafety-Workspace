// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.FMCSAStandards.AuditGaps
//  Layer:          Infrastructure
//  File:           FmcsaAuditGapDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing IFmcsaAuditGapRepository -- gap detection and the
//                  exposure dashboard queries built on top of the same coverage logic.
//                  GetSiteExposureAsync/GetTenantExposureAsync also write
//                  Compliance.FMCSAExposureSnapshot rows as a side effect (one per module
//                  returned), which is how exposure trend history accumulates. Mirrors
//                  OshaAuditGapDapperRepository exactly. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Infrastructure.Compliance.FMCSAStandards.AuditGaps.Persistence;

/// <summary>Dapper-backed repository for FMCSA requirement coverage gaps and exposure dashboards.</summary>
public sealed class FmcsaAuditGapDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ILogger<FmcsaAuditGapDapperRepository> logger)
    : IFmcsaAuditGapRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetGapsForSite Module {ModuleName} Site {SiteId} Tenant {TenantId}", moduleName, siteId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ModuleName", moduleName, 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<GapRow>(
            "[Compliance].[usp_FmcsaAuditGaps_GetForSite]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaGapSummaryDto>> GetGapsSummaryForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ModuleName", moduleName, 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_FmcsaAuditGaps_GetSummaryForSite]", parameters, cancellationToken);

        return rows.Select(ToSummaryDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var rows = await queryExecutor.QueryAsync<ModuleTotalRow>(
            "[Compliance].[usp_FmcsaDashboard_GetModuleTotals]", new StoredProcedureBuilder().Build(), cancellationToken);

        return rows.Select(r => new FmcsaModuleTotalDto
        {
            Module = r.Module,
            TotalRequirements = r.TotalRequirements,
            HighCount = r.HighCount,
            CriticalCount = r.CriticalCount,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_FmcsaDashboard_GetSiteExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_FmcsaDashboard_GetTenantExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId: null, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid? siteId, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<TopUncoveredRow>(
            "[Compliance].[usp_FmcsaDashboard_GetTopUncovered]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FmcsaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Module", module, 100)
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .Build();

        var rows = await queryExecutor.QueryAsync<SnapshotRow>(
            "[Compliance].[usp_FMCSAExposureSnapshot_GetTrend]", parameters, cancellationToken);

        return rows.Select(r => new FmcsaExposureSnapshotDto
        {
            SnapshotAtUtc = r.SnapshotAtUtc,
            Module = r.Module,
            TotalRequirements = r.TotalRequirements,
            CoveredCount = r.CoveredCount,
            GapCount = r.GapCount,
            HighGapCount = r.HighGapCount,
            CriticalGapCount = r.CriticalGapCount,
        }).ToList();
    }

    private async Task RecordSnapshotsAsync(Guid tenantId, Guid? siteId, IReadOnlyList<FmcsaGapSummaryDto> summaries, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        foreach (var summary in summaries)
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("SnapshotId", Guid.NewGuid())
                .AddGuid("TenantId", tenantId)
                .AddGuid("SiteId", siteId)
                .AddString("Module", summary.Module, 100)
                .AddInt("TotalRequirements", summary.TotalRequirements)
                .AddInt("CoveredCount", summary.CoveredCount)
                .AddInt("GapCount", summary.GapCount)
                .AddInt("HighGapCount", summary.HighGapCount)
                .AddInt("CriticalGapCount", summary.CriticalGapCount)
                .Build();

            await commandExecutor.ExecuteAsync(connection, "[Compliance].[usp_FMCSAExposureSnapshot_Record]", parameters, cancellationToken: cancellationToken);
        }
    }

    private static FmcsaGapDto ToGapDto(GapRow r) => new()
    {
        RequirementId = r.RequirementId,
        RequirementNumber = r.RequirementNumber,
        Severity = r.Severity,
        RequirementText = r.RequirementText,
        ModuleName = r.ModuleName,
        PartNumber = r.PartNumber,
        SectionNumber = r.SectionNumber,
        SectionTitle = r.SectionTitle,
    };

    private static FmcsaGapDto ToGapDto(TopUncoveredRow r) => new()
    {
        RequirementId = r.RequirementId,
        RequirementNumber = r.RequirementNumber,
        Severity = r.Severity,
        RequirementText = r.RequirementText,
        ModuleName = null,
        PartNumber = r.PartNumber,
        SectionNumber = r.SectionNumber,
        SectionTitle = r.SectionTitle,
    };

    private static FmcsaGapSummaryDto ToSummaryDto(SummaryRow row) => new()
    {
        Module = row.Module,
        TotalRequirements = row.TotalRequirements,
        CoveredCount = row.CoveredCount,
        GapCount = row.GapCount,
        HighGapCount = row.HighGapCount,
        CriticalGapCount = row.CriticalGapCount,
    };

    private sealed record GapRow
    {
        public required Guid RequirementId { get; init; }
        public string? RequirementNumber { get; init; }
        public required string Severity { get; init; }
        public required string RequirementText { get; init; }
        public string? ModuleName { get; init; }
        public required string PartNumber { get; init; }
        public required string SectionNumber { get; init; }
        public string? SectionTitle { get; init; }
    }

    private sealed record TopUncoveredRow
    {
        public required Guid RequirementId { get; init; }
        public string? RequirementNumber { get; init; }
        public required string Severity { get; init; }
        public required string RequirementText { get; init; }
        public required string PartNumber { get; init; }
        public required string SectionNumber { get; init; }
        public string? SectionTitle { get; init; }
    }

    private sealed record SummaryRow
    {
        public required string Module { get; init; }
        public required int TotalRequirements { get; init; }
        public required int CoveredCount { get; init; }
        public required int GapCount { get; init; }
        public required int HighGapCount { get; init; }
        public required int CriticalGapCount { get; init; }
    }

    private sealed record ModuleTotalRow
    {
        public required string Module { get; init; }
        public required int TotalRequirements { get; init; }
        public required int HighCount { get; init; }
        public required int CriticalCount { get; init; }
    }

    private sealed record SnapshotRow
    {
        public required DateTime SnapshotAtUtc { get; init; }
        public required string Module { get; init; }
        public required int TotalRequirements { get; init; }
        public required int CoveredCount { get; init; }
        public required int GapCount { get; init; }
        public required int HighGapCount { get; init; }
        public required int CriticalGapCount { get; init; }
    }
}
