// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.EnvironmentalHealth.Readings
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnvironmentalHealth read model. All DB access via
//                  stored procedures in the [EnvironmentalHealth] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.EnvironmentalHealth.Handlers.Readings;

namespace PlantSafety.Infrastructure.Features.EnvironmentalHealth.Readings.Repositories;

/// <summary>
/// Dapper-backed repository for EnvironmentalHealth readings.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class EnvironmentalHealthDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalHealthDapperRepository> logger)
    : IEnvironmentalHealthReadingRepository
{
    /// <inheritdoc/>
    public async Task<EnvironmentalHealthReadModel?> GetByIdAsync(Guid tenantId, Guid environmentalHealthId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get EnvironmentalHealth {EnvironmentalHealthId} for tenant {TenantId}", environmentalHealthId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("EnvironmentalHealthId", environmentalHealthId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EnvironmentalHealthReadModel>(
            "[EnvironmentalHealth].[usp_EnvironmentalHealth_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<EnvironmentalHealthReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? metricType,
        string? status,
        DateTime? from,
        DateTime? to,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List EnvironmentalHealth readings for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("MetricType", metricType, 50)
            .AddString("Status", status, 20)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<EnvironmentalHealthListRow>(
            "[EnvironmentalHealth].[usp_EnvironmentalHealth_List]", parameters, cancellationToken);

        var items = rows.Select(r => new EnvironmentalHealthReadModel
        {
            EnvironmentalHealthId = r.EnvironmentalHealthId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            MetricType = r.MetricType,
            MeasurementValue = r.MeasurementValue,
            Unit = r.Unit,
            Threshold = r.Threshold,
            Status = r.Status,
            RecordedAtUtc = r.RecordedAtUtc,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[EnvironmentalHealth].[usp_EnvironmentalHealth_List]</c>, including the windowed total row count.</summary>
    private sealed record EnvironmentalHealthListRow
    {
        public required Guid EnvironmentalHealthId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string MetricType { get; init; }
        public required decimal MeasurementValue { get; init; }
        public required string Unit { get; init; }
        public required decimal Threshold { get; init; }
        public required string Status { get; init; }
        public required DateTimeOffset RecordedAtUtc { get; init; }
        public required int TotalCount { get; init; }
    }
}
