// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Infrastructure
//  File:           ProductionTrackingReadDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer
//                  IProductionTrackingReadRepository (the module's full 13-endpoint query surface).
//                  Distinct from IProductionLineRepository/IProductionRunRepository (the Domain
//                  write-side aggregate repositories). Every list-style query uses the single-
//                  result-set COUNT(*) OVER() window-function pattern, mirroring
//                  WorkOrderReadDapperRepository, since DapperQueryExecutor exposes no multi-
//                  result-set method. GetActiveRunKpiInputsAsync/GetRunKpiInputAsync issue one
//                  additional per-run downtime-events query (small, bounded result sets; documented
//                  trade-off given the single-result-set constraint above). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.ProductionTracking.Models;
using PlantSafety.Application.Operational.ProductionTracking.Repositories;

namespace PlantSafety.Infrastructure.Operational.ProductionTracking.Persistence;

/// <summary>Dapper-backed repository for the ProductionTracking module's Application-layer query surface.</summary>
public sealed class ProductionTrackingReadDapperRepository(DapperQueryExecutor queryExecutor, ILogger<ProductionTrackingReadDapperRepository> logger)
    : IProductionTrackingReadRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductionRunKpiInputDto>> GetActiveRunKpiInputsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<ActiveRunKpiBaseRow>("[Operational].[usp_ProductionRun_ActiveKpiBase]", parameters, cancellationToken);

        var results = new List<ProductionRunKpiInputDto>(rows.Count);
        foreach (var row in rows)
        {
            var downtimeEvents = await GetDowntimeEventsAsync(tenantId, row.ProductionRunId, cancellationToken);
            results.Add(ToKpiInputDto(row, downtimeEvents));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ProductionRunRowDto> Items, int TotalCount)> ListActiveRunsAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<ActiveRunRow>("[Operational].[usp_ProductionRun_ListActive]", parameters, cancellationToken);

        var now = DateTime.UtcNow;
        var items = rows.Select(r => new ProductionRunRowDto
        {
            RunId = r.ProductionRunId,
            RunCode = r.RunCode,
            Line = r.LineName,
            StartUtc = AsUtcOffset(r.StartedAtUtc),
            Status = r.Status,
            Throughput = ComputeThroughput(r.GoodUnitCount, r.ScrapUnitCount, r.StartedAtUtc, r.EndedAtUtc, now),
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<RunSummaryDto?> GetRunSummaryAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var row = await GetRunRowAsync(tenantId, runId, cancellationToken);
        if (row is null) return null;

        var now = DateTime.UtcNow;
        return new RunSummaryDto
        {
            RunId = row.ProductionRunId,
            RunCode = row.RunCode,
            Status = row.Status,
            Throughput = ComputeThroughput(row.GoodUnitCount, row.ScrapUnitCount, row.StartedAtUtc, row.EndedAtUtc, now),
            TrendDirection = "Stable",
        };
    }

    /// <inheritdoc/>
    public async Task<RunMetadataDto?> GetRunMetadataAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var row = await GetRunRowAsync(tenantId, runId, cancellationToken);
        if (row is null) return null;

        return new RunMetadataDto
        {
            Line = row.LineName,
            StartUtc = AsUtcOffset(row.StartedAtUtc),
            Operator = row.OperatorName ?? string.Empty,
        };
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<TimelineEventDto> Items, int TotalCount)> GetRunTimelineAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddGuid("ProductionRunId", runId)
            .AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<TimelineRow>("[Operational].[usp_ProductionRun_Timeline]", parameters, cancellationToken);

        var items = rows.Select(r => new TimelineEventDto
        {
            EventId = r.EventId,
            TimestampUtc = AsUtcOffset(r.TimestampUtc),
            EventType = r.EventType,
            Description = r.Description,
            RunId = r.ProductionRunId,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<BatchRowDto> Items, int TotalCount)> GetRunBatchesAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddGuid("ProductionRunId", runId)
            .AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<BatchRow>("[Operational].[usp_ProductionBatch_ByRun]", parameters, cancellationToken);

        return (MapBatches(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<ProductionRunKpiInputDto?> GetRunKpiInputAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var row = await GetRunRowAsync(tenantId, runId, cancellationToken);
        if (row is null) return null;

        var downtimeEvents = await GetDowntimeEventsAsync(tenantId, runId, cancellationToken);
        return ToKpiInputDto(new ActiveRunKpiBaseRow
        {
            ProductionRunId = row.ProductionRunId,
            RunCode = row.RunCode,
            MachineId = row.MachineId,
            StartedAtUtc = row.StartedAtUtc,
            EndedAtUtc = row.EndedAtUtc,
            GoodUnitCount = row.GoodUnitCount,
            ScrapUnitCount = row.ScrapUnitCount,
            IdealCycleTimeSeconds = row.IdealCycleTimeSeconds,
            DowntimeMinutesTotal = row.DowntimeMinutesTotal,
        }, downtimeEvents);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<EventRowDto> Items, int TotalCount)> GetRunEventsAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddGuid("ProductionRunId", runId)
            .AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<EventRow>("[Operational].[usp_ProductionRun_Events]", parameters, cancellationToken);

        return (MapEvents(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<BatchRowDto> Items, int TotalCount)> ListBatchesAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<BatchRow>("[Operational].[usp_ProductionBatch_ListAll]", parameters, cancellationToken);

        return (MapBatches(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<TimelineEventDto> Items, int TotalCount)> GetProductionTimelineAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<TimelineRow>("[Operational].[usp_ProductionTimeline_PlantWide]", parameters, cancellationToken);

        var items = rows.Select(r => new TimelineEventDto
        {
            EventId = r.EventId,
            TimestampUtc = AsUtcOffset(r.TimestampUtc),
            EventType = r.EventType,
            Description = r.Description,
            RunId = r.ProductionRunId,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<EventRowDto> Items, int TotalCount)> GetEventHistoryAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<EventRow>("[Operational].[usp_ProductionEventHistory]", parameters, cancellationToken);

        return (MapEvents(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task<ProductionRunFullRow?> GetRunRowAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ProductionRunId", runId).AddGuid("TenantId", tenantId).Build();
        return await queryExecutor.QuerySingleOrDefaultAsync<ProductionRunFullRow>("[Operational].[usp_ProductionRun_GetById]", parameters, cancellationToken);
    }

    private async Task<IReadOnlyList<RunDowntimeEventRow>> GetDowntimeEventsAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("ProductionRunId", runId).Build();
        var rows = await queryExecutor.QueryAsync<DowntimeEventRow>("[Operational].[usp_ProductionDowntimeEvent_ListByRun]", parameters, cancellationToken);

        return rows.Select(r => new RunDowntimeEventRow
        {
            ReasonCode = r.ReasonCode,
            StartUtc = r.StartUtc,
            EndUtc = r.EndUtc,
            IsPlanned = r.IsPlanned,
        }).ToList();
    }

    private static ProductionRunKpiInputDto ToKpiInputDto(ActiveRunKpiBaseRow row, IReadOnlyList<RunDowntimeEventRow> downtimeEvents) => new()
    {
        RunId = row.ProductionRunId,
        RunCode = row.RunCode,
        MachineId = row.MachineId,
        StartedAtUtc = row.StartedAtUtc,
        EndedAtUtc = row.EndedAtUtc,
        GoodUnitCount = row.GoodUnitCount,
        ScrapUnitCount = row.ScrapUnitCount,
        IdealCycleTimeSeconds = row.IdealCycleTimeSeconds,
        DowntimeMinutesTotal = row.DowntimeMinutesTotal,
        DowntimeEvents = downtimeEvents,
    };

    private static List<BatchRowDto> MapBatches(IReadOnlyList<BatchRow> rows) =>
        rows.Select(r => new BatchRowDto
        {
            BatchId = r.BatchId,
            BatchCode = r.BatchCode,
            RunId = r.ProductionRunId,
            RunCode = r.RunCode,
            Line = r.LineName,
            Status = r.Status,
            StartUtc = AsUtcOffset(r.StartedAtUtc),
        }).ToList();

    private static List<EventRowDto> MapEvents(IReadOnlyList<EventRow> rows) =>
        rows.Select(r => new EventRowDto
        {
            EventId = r.EventId,
            TimestampUtc = AsUtcOffset(r.TimestampUtc),
            RunCode = r.RunCode,
            RunId = r.ProductionRunId,
            EventType = r.EventType,
            Description = r.Description,
        }).ToList();

    private static double ComputeThroughput(int goodUnitCount, int scrapUnitCount, DateTime startedAtUtc, DateTime? endedAtUtc, DateTime nowUtc)
    {
        var elapsedHours = (endedAtUtc ?? nowUtc).Subtract(startedAtUtc).TotalHours;
        if (elapsedHours <= 0) return 0.0;
        return Math.Round((goodUnitCount + scrapUnitCount) / elapsedHours, 2);
    }

    /// <summary>Converts a DB-sourced UTC <see cref="DateTime"/> (Kind=Unspecified) into a <see cref="DateTimeOffset"/>.</summary>
    private static DateTimeOffset AsUtcOffset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed record ActiveRunKpiBaseRow
    {
        public required Guid ProductionRunId { get; init; }
        public required string RunCode { get; init; }
        public required Guid MachineId { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? EndedAtUtc { get; init; }
        public required int GoodUnitCount { get; init; }
        public required int ScrapUnitCount { get; init; }
        public required double IdealCycleTimeSeconds { get; init; }
        public required double DowntimeMinutesTotal { get; init; }
    }

    private sealed record ActiveRunRow
    {
        public required Guid ProductionRunId { get; init; }
        public required string RunCode { get; init; }
        public required string LineName { get; init; }
        public required Guid MachineId { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? EndedAtUtc { get; init; }
        public required string Status { get; init; }
        public required int GoodUnitCount { get; init; }
        public required int ScrapUnitCount { get; init; }
        public required double IdealCycleTimeSeconds { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record ProductionRunFullRow
    {
        public string? ETag { get; init; }
        public required Guid ProductionRunId { get; init; }
        public required Guid TenantId { get; init; }
        public required string Status { get; init; }
        public required string RunCode { get; init; }
        public required Guid LineId { get; init; }
        public required string LineName { get; init; }
        public required Guid MachineId { get; init; }
        public string? OperatorName { get; init; }
        public required double IdealCycleTimeSeconds { get; init; }
        public required int GoodUnitCount { get; init; }
        public required int ScrapUnitCount { get; init; }
        public required double DowntimeMinutesTotal { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? EndedAtUtc { get; init; }
    }

    private sealed record TimelineRow
    {
        public required Guid EventId { get; init; }
        public required DateTime TimestampUtc { get; init; }
        public required string EventType { get; init; }
        public required string Description { get; init; }
        public required Guid ProductionRunId { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record EventRow
    {
        public required Guid EventId { get; init; }
        public required DateTime TimestampUtc { get; init; }
        public required string RunCode { get; init; }
        public required Guid ProductionRunId { get; init; }
        public required string EventType { get; init; }
        public required string Description { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record BatchRow
    {
        public required Guid BatchId { get; init; }
        public required string BatchCode { get; init; }
        public required Guid ProductionRunId { get; init; }
        public required string RunCode { get; init; }
        public required string LineName { get; init; }
        public required string Status { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record DowntimeEventRow
    {
        public required string ReasonCode { get; init; }
        public required DateTime StartUtc { get; init; }
        public DateTime? EndUtc { get; init; }
        public required bool IsPlanned { get; init; }
    }
}
