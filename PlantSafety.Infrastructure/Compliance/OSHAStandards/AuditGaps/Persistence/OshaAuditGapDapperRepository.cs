// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.OSHAStandards.AuditGaps
//  Layer:          Infrastructure
//  File:           OshaAuditGapDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing IOshaAuditGapRepository -- gap detection and, as of
//                  2026-08-07, the exposure dashboard queries built on top of the same coverage
//                  logic. GetSiteExposureAsync/GetTenantExposureAsync also write
//                  Compliance.OSHAExposureSnapshot rows as a side effect (one per module
//                  returned), which is how exposure trend history accumulates -- see
//                  OSHAExposureSnapshot.sql's header. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

namespace PlantSafety.Infrastructure.Compliance.OSHAStandards.AuditGaps.Persistence;

/// <summary>Dapper-backed repository for OSHA requirement coverage gaps and exposure dashboards.</summary>
public sealed class OshaAuditGapDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ILogger<OshaAuditGapDapperRepository> logger)
    : IOshaAuditGapRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid siteId, string module, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetGapsForSite Module {Module} Site {SiteId} Tenant {TenantId}", module, siteId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Module", module, 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<GapRow>(
            "[Compliance].[usp_OshaAuditGaps_GetForSite]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<OshaGapSummaryDto> GetGapsSummaryForSiteAsync(Guid tenantId, Guid siteId, string module, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Module", module, 100)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SummaryRow>(
            "[Compliance].[usp_OshaAuditGaps_GetSummaryForSite]", parameters, cancellationToken);

        return row is null ? EmptySummary(module) : ToSummaryDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var rows = await queryExecutor.QueryAsync<ModuleTotalRow>(
            "[Compliance].[usp_OshaDashboard_GetModuleTotals]", new StoredProcedureBuilder().Build(), cancellationToken);

        return rows.Select(r => new OshaModuleTotalDto
        {
            Module = r.Module,
            TotalRequirements = r.TotalRequirements,
            HighCount = r.HighCount,
            CriticalCount = r.CriticalCount,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_OshaDashboard_GetSiteExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var rows = await queryExecutor.QueryAsync<SummaryRow>(
            "[Compliance].[usp_OshaDashboard_GetTenantExposure]", parameters, cancellationToken);

        var summaries = rows.Select(ToSummaryDto).ToList();
        await RecordSnapshotsAsync(tenantId, siteId: null, summaries, cancellationToken);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid siteId, int take, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("Take", take)
            .Build();

        var rows = await queryExecutor.QueryAsync<GapRow>(
            "[Compliance].[usp_OshaDashboard_GetTopUncovered]", parameters, cancellationToken);

        return rows.Select(ToGapDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OshaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Module", module, 100)
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .Build();

        var rows = await queryExecutor.QueryAsync<SnapshotRow>(
            "[Compliance].[usp_OSHAExposureSnapshot_GetTrend]", parameters, cancellationToken);

        return rows.Select(r => new OshaExposureSnapshotDto
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

    private async Task RecordSnapshotsAsync(Guid tenantId, Guid? siteId, IReadOnlyList<OshaGapSummaryDto> summaries, CancellationToken cancellationToken)
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

            await commandExecutor.ExecuteAsync(connection, "[Compliance].[usp_OSHAExposureSnapshot_Record]", parameters, cancellationToken: cancellationToken);
        }
    }

    private static OshaGapDto ToGapDto(GapRow r) => new()
    {
        OSHARequirementId = r.OSHARequirementId,
        RequirementType = r.RequirementType,
        Severity = r.Severity,
        Notes = r.Notes,
        PartNumber = r.PartNumber,
        Section = r.Section,
        SectionTitle = r.SectionTitle,
    };

    private static OshaGapSummaryDto ToSummaryDto(SummaryRow row) => new()
    {
        Module = row.Module,
        TotalRequirements = row.TotalRequirements,
        CoveredCount = row.CoveredCount,
        GapCount = row.GapCount,
        HighGapCount = row.HighGapCount,
        CriticalGapCount = row.CriticalGapCount,
    };

    private static OshaGapSummaryDto EmptySummary(string module) =>
        new() { Module = module, TotalRequirements = 0, CoveredCount = 0, GapCount = 0, HighGapCount = 0, CriticalGapCount = 0 };

    private sealed record GapRow
    {
        public required Guid OSHARequirementId { get; init; }
        public string? RequirementType { get; init; }
        public required string Severity { get; init; }
        public required string Notes { get; init; }
        public required int PartNumber { get; init; }
        public required string Section { get; init; }
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
