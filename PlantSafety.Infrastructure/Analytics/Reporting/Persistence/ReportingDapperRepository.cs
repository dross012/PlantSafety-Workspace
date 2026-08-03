// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Reporting
//  Layer:          Infrastructure
//  File:           ReportingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Reporting read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Analytics.Reporting.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Reporting.Persistence;

/// <summary>Dapper-backed repository for <see cref="ReportingItemDto"/> queries.</summary>
public sealed class ReportingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ReportingDapperRepository> logger)
    : IReportingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReportingItemDto>> GetReportItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetReportItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ReportingItemDto>(
            "[Analytics].[usp_Reporting_List]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ReportListItemDto> Items, int TotalCount)> GetReportsPagedAsync(
        Guid tenantId,
        Guid? siteId,
        string? typeFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetReportsPagedAsync for tenant {TenantId}", tenantId);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ReportType", typeFilter, 100)
            .Build();

        var pageParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ReportType", typeFilter, 100)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int>(
            "[Analytics].[usp_Reporting_GetListCount]", countParameters, cancellationToken);

        var rows = await queryExecutor.QueryAsync<ReportListRow>(
            "[Analytics].[usp_Reporting_GetListPage]", pageParameters, cancellationToken);

        return (rows.Select(ToListItemDto).ToList(), totalCount);
    }

    /// <inheritdoc/>
    public async Task<ReportDetailDto?> GetReportByIdAsync(
        Guid tenantId,
        Guid reportId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetReportByIdAsync {ReportId} for tenant {TenantId}", reportId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ReportItemId", reportId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ReportDetailRow>(
            "[Analytics].[usp_Reporting_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDetailDto(row);
    }

    // ---- Mapping helpers ---------------------------------------------------------------

    private static ReportListItemDto ToListItemDto(ReportListRow row) => new()
    {
        ReportId = row.ReportItemId,
        Title = row.ReportName,
        ReportType = row.ReportCategory,
        Status = row.Status,
        GeneratedAtUtc = DateTime.SpecifyKind(row.GeneratedAtUtc, DateTimeKind.Utc),
        OwnerName = row.OwnerName ?? string.Empty,
    };

    private static ReportDetailDto ToDetailDto(ReportDetailRow row) => new()
    {
        ReportId = row.ReportItemId,
        Title = row.ReportName,
        Description = row.Description ?? string.Empty,
        ReportType = row.ReportCategory,
        Status = row.Status,
        PeriodStart = row.PeriodStartUtc.HasValue ? DateOnly.FromDateTime(row.PeriodStartUtc.Value) : DateOnly.FromDateTime(row.GeneratedAtUtc),
        PeriodEnd = row.PeriodEndUtc.HasValue ? DateOnly.FromDateTime(row.PeriodEndUtc.Value) : DateOnly.FromDateTime(row.GeneratedAtUtc),
        GeneratedAtUtc = DateTime.SpecifyKind(row.GeneratedAtUtc, DateTimeKind.Utc),
        OwnerName = row.OwnerName ?? string.Empty,
        AvailableFormats = string.IsNullOrWhiteSpace(row.AvailableFormats)
            ? Array.Empty<string>()
            : row.AvailableFormats.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        SourceModule = row.SourceModule ?? string.Empty,
    };

    // ---- Row types (raw stored-procedure projections) ---------------------------------

    private sealed class ReportListRow
    {
        public Guid ReportItemId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportCategory { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedAtUtc { get; set; }
        public string? OwnerName { get; set; }
    }

    private sealed class ReportDetailRow
    {
        public Guid ReportItemId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ReportCategory { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PeriodStartUtc { get; set; }
        public DateTime? PeriodEndUtc { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public string? OwnerName { get; set; }
        public string? AvailableFormats { get; set; }
        public string? SourceModule { get; set; }
    }
}
