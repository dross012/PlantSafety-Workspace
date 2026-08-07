// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.NFPAStandards.AuditGaps
//  Layer:          Infrastructure
//  File:           NfpaAuditGapDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing INfpaAuditGapRepository -- gap detection and the
//                  exposure dashboard queries built on top of the same coverage logic.
//                  GetSiteExposureAsync/GetTenantExposureAsync also write
//                  Compliance.NFPAExposureSnapshot rows as a side effect (one per module
//                  returned), which is how exposure trend history accumulates -- see
//                  NFPAExposureSnapshot.sql's header. Mirrors OshaAuditGapDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

namespace PlantSafety.Infrastructure.Compliance.NFPAStandards.AuditGaps.Persistence;

/// <summary>Dapper-backed repository for NFPA hazard mapping coverage gaps and exposure dashboards.</summary>
public sealed class NfpaAuditGapDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ILogger<NfpaAuditGapDapperRepository> logger)
    : INfpaAuditGapRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetGapsForSite Module {ModuleName} Site {SiteId} Tenant {TenantId}", moduleName, siteId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ModuleName", moduleName, 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<GapRow>(
            "[Compliance].[usp_NfpaAuditGaps_GetForSite]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaGapSummaryDto>> GetGapsSummaryForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ModuleName", moduleName, 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_NfpaAuditGaps_GetSummaryForSite]", parameters, cancellationToken);

        return rows.Select(ToSummaryDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var rows = await queryExecutor.QueryAsync<ModuleTotalRow>(
            "[Compliance].[usp_NfpaDashboard_GetModuleTotals]", new StoredProcedureBuilder().Build(), cancellationToken);

        return rows.Select(r => new NfpaModuleTotalDto
        {
            Module = r.Module,
            TotalHazards = r.TotalHazards,
            HighCount = r.HighCount,
            CriticalCount = r.CriticalCount,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_NfpaDashboard_GetSiteExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_NfpaDashboard_GetTenantExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId: null, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid? siteId, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<GapRow>(
            "[Compliance].[usp_NfpaDashboard_GetTopUncovered]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NfpaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Module", module, 100)
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .Build();

        var rows = await queryExecutor.QueryAsync<SnapshotRow>(
            "[Compliance].[usp_NFPAExposureSnapshot_GetTrend]", parameters, cancellationToken);

        return rows.Select(r => new NfpaExposureSnapshotDto
        {
            SnapshotAtUtc = r.SnapshotAtUtc,
            Module = r.Module,
            TotalHazards = r.TotalHazards,
            CoveredCount = r.CoveredCount,
            GapCount = r.GapCount,
            HighGapCount = r.HighGapCount,
            CriticalGapCount = r.CriticalGapCount,
        }).ToList();
    }

    private async Task RecordSnapshotsAsync(Guid tenantId, Guid? siteId, IReadOnlyList<NfpaGapSummaryDto> summaries, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        foreach (var summary in summaries)
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("SnapshotId", Guid.NewGuid())
                .AddGuid("TenantId", tenantId)
                .AddGuid("SiteId", siteId)
                .AddString("Module", summary.Module, 100)
                .AddInt("TotalHazards", summary.TotalHazards)
                .AddInt("CoveredCount", summary.CoveredCount)
                .AddInt("GapCount", summary.GapCount)
                .AddInt("HighGapCount", summary.HighGapCount)
                .AddInt("CriticalGapCount", summary.CriticalGapCount)
                .Build();

            await commandExecutor.ExecuteAsync(connection, "[Compliance].[usp_NFPAExposureSnapshot_Record]", parameters, cancellationToken: cancellationToken);
        }
    }

    private static NfpaGapDto ToGapDto(GapRow r) => new()
    {
        MappingId = r.MappingId,
        ModuleName = r.ModuleName,
        MappingRuleCode = r.MappingRuleCode,
        Severity = r.Severity,
        Rationale = r.Rationale,
        HazardId = r.HazardId,
        CodeNumber = r.CodeNumber,
        SectionNumber = r.SectionNumber,
        HazardName = r.HazardName,
    };

    private static NfpaGapSummaryDto ToSummaryDto(SummaryRow row) => new()
    {
        Module = row.Module,
        TotalHazards = row.TotalHazards,
        CoveredCount = row.CoveredCount,
        GapCount = row.GapCount,
        HighGapCount = row.HighGapCount,
        CriticalGapCount = row.CriticalGapCount,
    };

    private sealed record GapRow
    {
        public required Guid MappingId { get; init; }
        public required string ModuleName { get; init; }

        // Nullable: usp_NfpaDashboard_GetTopUncovered does not select MappingRuleCode (it spans
        // every module and omits per-mapping rule provenance by design), while
        // usp_NfpaAuditGaps_GetForSite/usp_NfpaDashboard_GetTopUncovered share this row type --
        // Dapper leaves this null when the backing result set omits the column.
        public string? MappingRuleCode { get; init; }
        public required string Severity { get; init; }
        public required string Rationale { get; init; }
        public required Guid HazardId { get; init; }
        public required string CodeNumber { get; init; }
        public required string SectionNumber { get; init; }
        public required string HazardName { get; init; }
    }

    private sealed record SummaryRow
    {
        public required string Module { get; init; }
        public required int TotalHazards { get; init; }
        public required int CoveredCount { get; init; }
        public required int GapCount { get; init; }
        public required int HighGapCount { get; init; }
        public required int CriticalGapCount { get; init; }
    }

    private sealed record ModuleTotalRow
    {
        public required string Module { get; init; }
        public required int TotalHazards { get; init; }
        public required int HighCount { get; init; }
        public required int CriticalCount { get; init; }
    }

    private sealed record SnapshotRow
    {
        public required DateTime SnapshotAtUtc { get; init; }
        public required string Module { get; init; }
        public required int TotalHazards { get; init; }
        public required int CoveredCount { get; init; }
        public required int GapCount { get; init; }
        public required int HighGapCount { get; init; }
        public required int CriticalGapCount { get; init; }
    }
}
