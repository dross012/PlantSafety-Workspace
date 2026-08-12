// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Infrastructure
//  File:           CognitiveLoadLedgerDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ICognitiveLoadLedgerRepository. All DB access
//                  via stored procedures in the [HumanFactorsCognitiveLoadAnalytics] schema.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because
//                  CognitiveLoadLedger exposes only a private "for rehydration only" constructor
//                  and private-setter properties/backing-collection fields. AddAsync AND
//                  UpdateAsync both re-submit the aggregate's full in-memory child collections on
//                  every call -- each child Insert proc is idempotent (IF NOT EXISTS guard), so
//                  this is safe and avoids tracking a separate "dirty" list on the aggregate.
//                  Mirrors AdvancedMetricsLedgerDapperRepository's precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Specifications;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.Persistence;

/// <summary>Dapper-backed repository for the CognitiveLoadLedger aggregate root.</summary>
public sealed class CognitiveLoadLedgerDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : ICognitiveLoadLedgerRepository
{
    /// <inheritdoc/>
    public async Task<CognitiveLoadLedger?> GetByIdAsync(CognitiveLoadLedgerId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("CognitiveLoadLedgerId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<LedgerRow>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_GetById]", parameters, cancellationToken);

        return row is null ? null : await HydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CognitiveLoadLedger?> GetOpenLedgerAsync(
        Guid tenantId, Guid? siteId, ShiftType shiftType, UtcTimestamp shiftDateUtc, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ShiftType", shiftType.ToString(), 20)
            .AddDateTime("ShiftDateUtc", shiftDateUtc.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LedgerRow>(
            "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_GetOpenByShift]", parameters, cancellationToken);

        return row is null ? null : await HydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CognitiveLoadLedger>> ListAsync(CognitiveLoadLedgerSpecification specification, CancellationToken cancellationToken = default)
    {
        // Candidate set is per-tenant; the specification's ToPredicate() is applied in-memory,
        // matching the Specifications pattern used elsewhere in the codebase. Not supported in
        // this build-out's SQL surface -- callers needing filtered lists should follow up with
        // GetByIdAsync/GetOpenLedgerAsync, mirroring AdvancedMetricsLedgerDapperRepository's
        // identical ListAsync precedent.
        throw new NotSupportedException(
            "CognitiveLoadLedgerSpecification.ListAsync requires a TenantId-bearing specification variant; " +
            "use GetByIdAsync or GetOpenLedgerAsync for the supported access paths in this build-out.");
    }

    /// <inheritdoc/>
    public async Task AddAsync(CognitiveLoadLedger aggregate, CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParameters = new StoredProcedureBuilder()
            .AddGuid("CognitiveLoadLedgerId", aggregate.CognitiveLoadLedgerId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("ShiftType", aggregate.ShiftType.ToString(), 20)
            .AddDateTime("ShiftDateUtc", aggregate.ShiftDateUtc.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_Insert]", headerParameters, tx, cancellationToken: cancellationToken);

        await PersistChildrenAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(CognitiveLoadLedger aggregate, CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        if (aggregate.Status != CognitiveLoadLedgerStatus.Open)
        {
            var statusParameters = new StoredProcedureBuilder()
                .AddGuid("CognitiveLoadLedgerId", aggregate.CognitiveLoadLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Status", aggregate.Status.ToString(), 50)
                .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
                .Build();

            await commandExecutor.ExecuteScalarAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_UpdateStatus]", statusParameters, tx, cancellationToken: cancellationToken);
        }

        await PersistChildrenAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    private async Task PersistChildrenAsync(IDbConnection conn, IDbTransaction tx, CognitiveLoadLedger aggregate, CancellationToken cancellationToken)
    {
        foreach (var reading in aggregate.CognitiveLoadReadings)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("CognitiveLoadReadingId", reading.CognitiveLoadReadingId.Value)
                .AddGuid("CognitiveLoadLedgerId", aggregate.CognitiveLoadLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("WorkerUserId", reading.WorkerUserId.Value)
                .AddGuid("TaskComplexityProfileId", reading.TaskComplexityProfileId?.Value)
                .AddDecimal("CognitiveLoadScore", reading.CognitiveLoadScore)
                .AddString("WorkloadLevel", reading.WorkloadLevel.ToString(), 20)
                .AddString("MeasurementMethod", reading.MeasurementMethod, 100)
                .AddDateTime("MeasuredAtUtc", reading.MeasuredAtUtc.Value)
                .AddDateTime("RecordedAtUtc", reading.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", reading.RecordedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var profile in aggregate.TaskComplexityProfiles)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("TaskComplexityProfileId", profile.TaskComplexityProfileId.Value)
                .AddGuid("CognitiveLoadLedgerId", aggregate.CognitiveLoadLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("TaskCode", profile.TaskCode, 50)
                .AddString("TaskName", profile.TaskName, 200)
                .AddInt("ComplexityScore", profile.ComplexityScore)
                .AddString("Description", profile.Description, 1000)
                .AddDateTime("DefinedAtUtc", profile.DefinedAtUtc.Value)
                .AddGuid("DefinedBy", profile.DefinedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var trend in aggregate.WorkloadTrendRecords)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("WorkloadTrendRecordId", trend.WorkloadTrendRecordId.Value)
                .AddGuid("CognitiveLoadLedgerId", aggregate.CognitiveLoadLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddDateTime("PeriodStartUtc", trend.PeriodStartUtc.Value)
                .AddDateTime("PeriodEndUtc", trend.PeriodEndUtc.Value)
                .AddDecimal("MeanCognitiveLoadScore", trend.MeanCognitiveLoadScore)
                .AddInt("OverloadEventCount", trend.OverloadEventCount)
                .AddInt("ReadingCount", trend.ReadingCount)
                .AddDateTime("RecalculatedAtUtc", trend.RecalculatedAtUtc.Value)
                .AddGuid("RecalculatedBy", trend.RecalculatedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[HumanFactorsCognitiveLoadAnalytics].[usp_WorkloadTrendRecord_Insert]", p, tx, cancellationToken: cancellationToken);
        }
    }

    private async Task<CognitiveLoadLedger> HydrateAsync(LedgerRow row, CancellationToken cancellationToken)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CognitiveLoadLedger>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.CognitiveLoadLedgerId), CognitiveLoadLedgerId.FromGuid(row.CognitiveLoadLedgerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.ShiftType), Enum.Parse<ShiftType>(row.ShiftType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.ShiftDateUtc), UtcTimestamp.From(row.ShiftDateUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.Status), Enum.Parse<CognitiveLoadLedgerStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CognitiveLoadLedger.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICognitiveLoadAnalyticsDomainEvent>());

        var listParameters = new StoredProcedureBuilder().AddGuid("CognitiveLoadLedgerId", row.CognitiveLoadLedgerId).AddGuid("TenantId", row.TenantId).Build();

        var readingRows = await queryExecutor.QueryAsync<CognitiveLoadReadingRow>("[HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_cognitiveLoadReadings", readingRows.Select(r => new CognitiveLoadReading(
            CognitiveLoadReadingId.New(), new UserId(r.WorkerUserId),
            r.TaskComplexityProfileId is { } profileId ? TaskComplexityProfileId.FromGuid(profileId) : null,
            r.CognitiveLoadScore, Enum.Parse<CognitiveWorkloadLevel>(r.WorkloadLevel), r.MeasurementMethod,
            UtcTimestamp.From(r.MeasuredAtUtc), UtcTimestamp.From(r.RecordedAtUtc), new UserId(r.RecordedBy))).ToList());

        var profileRows = await queryExecutor.QueryAsync<TaskComplexityProfileRow>("[HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_taskComplexityProfiles", profileRows.Select(r => new TaskComplexityProfile(
            TaskComplexityProfileId.New(), r.TaskCode, r.TaskName, r.ComplexityScore, r.Description,
            UtcTimestamp.From(r.DefinedAtUtc), new UserId(r.DefinedBy))).ToList());

        var trendRows = await queryExecutor.QueryAsync<WorkloadTrendRecordRow>("[HumanFactorsCognitiveLoadAnalytics].[usp_WorkloadTrendRecord_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_workloadTrendRecords", trendRows.Select(r => new WorkloadTrendRecord(
            WorkloadTrendRecordId.New(), UtcTimestamp.From(r.PeriodStartUtc), UtcTimestamp.From(r.PeriodEndUtc),
            r.MeanCognitiveLoadScore, r.OverloadEventCount, r.ReadingCount,
            UtcTimestamp.From(r.RecalculatedAtUtc), new UserId(r.RecalculatedBy))).ToList());

        return agg;
    }

    private void DispatchDomainEvents(CognitiveLoadLedger aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private sealed record LedgerRow
    {
        public required Guid CognitiveLoadLedgerId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ShiftType { get; init; }
        public required DateTime ShiftDateUtc { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }

    private sealed record CognitiveLoadReadingRow
    {
        public required Guid WorkerUserId { get; init; }
        public Guid? TaskComplexityProfileId { get; init; }
        public decimal CognitiveLoadScore { get; init; }
        public required string WorkloadLevel { get; init; }
        public required string MeasurementMethod { get; init; }
        public required DateTime MeasuredAtUtc { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    private sealed record TaskComplexityProfileRow
    {
        public required string TaskCode { get; init; }
        public required string TaskName { get; init; }
        public int ComplexityScore { get; init; }
        public required string Description { get; init; }
        public required DateTime DefinedAtUtc { get; init; }
        public required Guid DefinedBy { get; init; }
    }

    private sealed record WorkloadTrendRecordRow
    {
        public required DateTime PeriodStartUtc { get; init; }
        public required DateTime PeriodEndUtc { get; init; }
        public decimal MeanCognitiveLoadScore { get; init; }
        public int OverloadEventCount { get; init; }
        public int ReadingCount { get; init; }
        public required DateTime RecalculatedAtUtc { get; init; }
        public required Guid RecalculatedBy { get; init; }
    }
}
