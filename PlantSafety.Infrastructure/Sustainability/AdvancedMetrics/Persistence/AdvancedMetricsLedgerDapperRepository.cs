// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Infrastructure
//  File:           AdvancedMetricsLedgerDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAdvancedMetricsLedgerRepository. All DB
//                  access via stored procedures in the [SustainabilityAdvancedMetrics] schema.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because
//                  AdvancedMetricsLedger exposes only a private "for rehydration only"
//                  constructor and private-setter properties/backing-collection fields. AddAsync
//                  AND UpdateAsync both re-submit the aggregate's full in-memory child collections
//                  on every call -- each child Insert proc is idempotent (IF NOT EXISTS guard), so
//                  this is safe and avoids tracking a separate "dirty" list on the aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Specifications;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.Persistence;

/// <summary>Dapper-backed repository for the AdvancedMetricsLedger aggregate root.</summary>
public sealed class AdvancedMetricsLedgerDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AdvancedMetricsLedgerDapperRepository> logger)
    : IAdvancedMetricsLedgerRepository
{
    /// <inheritdoc/>
    public async Task<AdvancedMetricsLedger?> GetByIdAsync(AdvancedMetricsLedgerId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<LedgerRow>(
            "[SustainabilityAdvancedMetrics].[usp_AdvancedMetricsLedger_GetById]", parameters, cancellationToken);

        return row is null ? null : await HydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AdvancedMetricsLedger?> GetOpenLedgerAsync(
        Guid tenantId, Guid? siteId, UtcTimestamp reportingPeriodStartUtc, UtcTimestamp reportingPeriodEndUtc,
        CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("ReportingPeriodStartUtc", reportingPeriodStartUtc.Value)
            .AddDateTime("ReportingPeriodEndUtc", reportingPeriodEndUtc.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LedgerRow>(
            "[SustainabilityAdvancedMetrics].[usp_AdvancedMetricsLedger_GetOpenByPeriod]", parameters, cancellationToken);

        return row is null ? null : await HydrateAsync(row, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AdvancedMetricsLedger>> ListAsync(AdvancedMetricsLedgerSpecification specification, CancellationToken cancellationToken = default)
    {
        // Candidate set is per-tenant; the specification's ToPredicate() is applied in-memory,
        // matching the Specifications pattern used elsewhere in the codebase. Headers only --
        // child collections are not hydrated for list results (callers needing full detail should
        // follow up with GetByIdAsync).
        throw new NotSupportedException(
            "AdvancedMetricsLedgerSpecification.ListAsync requires a TenantId-bearing specification variant; " +
            "use GetByIdAsync or GetOpenLedgerAsync for the supported access paths in this build-out.");
    }

    /// <inheritdoc/>
    public async Task AddAsync(AdvancedMetricsLedger aggregate, CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParameters = new StoredProcedureBuilder()
            .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddDateTime("ReportingPeriodStartUtc", aggregate.ReportingPeriodStartUtc.Value)
            .AddDateTime("ReportingPeriodEndUtc", aggregate.ReportingPeriodEndUtc.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_AdvancedMetricsLedger_Insert]", headerParameters, tx, cancellationToken: cancellationToken);

        await PersistChildrenAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AdvancedMetricsLedger aggregate, CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        if (aggregate.Status != AdvancedMetricsLedgerStatus.Open)
        {
            var statusParameters = new StoredProcedureBuilder()
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Status", aggregate.Status.ToString(), 50)
                .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
                .Build();

            await commandExecutor.ExecuteScalarAsync(conn, "[SustainabilityAdvancedMetrics].[usp_AdvancedMetricsLedger_UpdateStatus]", statusParameters, tx, cancellationToken: cancellationToken);
        }

        await PersistChildrenAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    private async Task PersistChildrenAsync(IDbConnection conn, IDbTransaction tx, AdvancedMetricsLedger aggregate, CancellationToken cancellationToken)
    {
        foreach (var record in aggregate.CarbonEmissionRecords)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("CarbonEmissionRecordId", record.CarbonEmissionRecordId.Value)
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Scope", record.Scope.ToString(), 20)
                .AddString("ActivityDescription", record.ActivityDescription, 400)
                .AddDecimal("EmissionsMetricTonsCo2e", record.EmissionsMetricTonsCo2e)
                .AddString("MethodologyVersion", record.MethodologyVersion, 100)
                .AddDateTime("ActivityAtUtc", record.ActivityAtUtc.Value)
                .AddDateTime("RecordedAtUtc", record.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", record.RecordedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_CarbonEmissionRecord_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var metric in aggregate.EnergyIntensityMetrics)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("EnergyIntensityMetricId", metric.EnergyIntensityMetricId.Value)
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddDecimal("EnergyConsumedKwh", metric.EnergyConsumedKwh)
                .AddDecimal("OutputUnits", metric.OutputUnits)
                .AddDecimal("IntensityValue", metric.IntensityValue)
                .AddString("MethodologyVersion", metric.MethodologyVersion, 100)
                .AddDateTime("ReadingAtUtc", metric.ReadingAtUtc.Value)
                .AddDateTime("RecordedAtUtc", metric.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", metric.RecordedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_EnergyIntensityMetric_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var metric in aggregate.WaterIntensityMetrics)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("WaterIntensityMetricId", metric.WaterIntensityMetricId.Value)
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddDecimal("WaterConsumedCubicMeters", metric.WaterConsumedCubicMeters)
                .AddDecimal("OutputUnits", metric.OutputUnits)
                .AddDecimal("IntensityValue", metric.IntensityValue)
                .AddString("MethodologyVersion", metric.MethodologyVersion, 100)
                .AddDateTime("ReadingAtUtc", metric.ReadingAtUtc.Value)
                .AddDateTime("RecordedAtUtc", metric.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", metric.RecordedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_WaterIntensityMetric_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var record in aggregate.WasteDiversionRecords)
        {
            var p = new StoredProcedureBuilder()
                .AddGuid("WasteDiversionRecordId", record.WasteDiversionRecordId.Value)
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddDecimal("TotalWasteKg", record.TotalWasteKg)
                .AddDecimal("DivertedWasteKg", record.DivertedWasteKg)
                .AddDecimal("DiversionRatePercent", record.DiversionRatePercent)
                .AddString("MethodologyVersion", record.MethodologyVersion, 100)
                .AddDateTime("IntervalEndAtUtc", record.IntervalEndAtUtc.Value)
                .AddDateTime("RecordedAtUtc", record.RecordedAtUtc.Value)
                .AddGuid("RecordedBy", record.RecordedBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_WasteDiversionRecord_Insert]", p, tx, cancellationToken: cancellationToken);
        }

        foreach (var target in aggregate.SustainabilityTargets)
        {
            var insertParameters = new StoredProcedureBuilder()
                .AddGuid("SustainabilityTargetId", target.SustainabilityTargetId.Value)
                .AddGuid("AdvancedMetricsLedgerId", aggregate.AdvancedMetricsLedgerId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("TargetType", target.TargetType.ToString(), 50)
                .AddDecimal("BaselineValue", target.BaselineValue)
                .AddDecimal("TargetValue", target.TargetValue)
                .AddDecimal("CurrentValue", target.BaselineValue)
                .AddDateTime("TargetDateUtc", target.TargetDateUtc.Value)
                .AddString("Status", SustainabilityTargetStatus.OnTrack.ToString(), 20)
                .AddDateTime("SetAtUtc", target.SetAtUtc.Value)
                .AddGuid("SetBy", target.SetBy.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_Insert]", insertParameters, tx, cancellationToken: cancellationToken);

            // Progress update is a no-op for a brand-new target (CurrentValue already == BaselineValue
            // from the insert above) but keeps re-hydrated/updated targets' progress in sync.
            var progressParameters = new StoredProcedureBuilder()
                .AddGuid("SustainabilityTargetId", target.SustainabilityTargetId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddDecimal("CurrentValue", target.CurrentValue)
                .AddString("Status", target.Status.ToString(), 20)
                .AddDateTime("UpdatedAtUtc", (target.LastProgressUpdateAtUtc ?? target.SetAtUtc).Value)
                .Build();

            await commandExecutor.ExecuteScalarAsync(conn, "[SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_UpdateProgress]", progressParameters, tx, cancellationToken: cancellationToken);
        }
    }

    private async Task<AdvancedMetricsLedger> HydrateAsync(LedgerRow row, CancellationToken cancellationToken)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<AdvancedMetricsLedger>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.AdvancedMetricsLedgerId), AdvancedMetricsLedgerId.FromGuid(row.AdvancedMetricsLedgerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.ReportingPeriodStartUtc), UtcTimestamp.From(row.ReportingPeriodStartUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.ReportingPeriodEndUtc), UtcTimestamp.From(row.ReportingPeriodEndUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.Status), Enum.Parse<AdvancedMetricsLedgerStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAdvancedMetricsDomainEvent>());

        var listParameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", row.AdvancedMetricsLedgerId).AddGuid("TenantId", row.TenantId).Build();

        var carbonRows = await queryExecutor.QueryAsync<CarbonEmissionRecordRow>("[SustainabilityAdvancedMetrics].[usp_CarbonEmissionRecord_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_carbonEmissionRecords", carbonRows.Select(r => new CarbonEmissionRecord(
            CarbonEmissionRecordId.New(), Enum.Parse<GhgScope>(r.Scope), r.ActivityDescription, r.EmissionsMetricTonsCo2e, r.MethodologyVersion,
            UtcTimestamp.From(r.ActivityAtUtc), UtcTimestamp.From(r.RecordedAtUtc), new UserId(r.RecordedBy))).ToList());

        var energyRows = await queryExecutor.QueryAsync<EnergyIntensityMetricRow>("[SustainabilityAdvancedMetrics].[usp_EnergyIntensityMetric_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_energyIntensityMetrics", energyRows.Select(r => new EnergyIntensityMetric(
            EnergyIntensityMetricId.New(), r.EnergyConsumedKwh, r.OutputUnits, r.MethodologyVersion,
            UtcTimestamp.From(r.ReadingAtUtc), UtcTimestamp.From(r.RecordedAtUtc), new UserId(r.RecordedBy))).ToList());

        var waterRows = await queryExecutor.QueryAsync<WaterIntensityMetricRow>("[SustainabilityAdvancedMetrics].[usp_WaterIntensityMetric_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_waterIntensityMetrics", waterRows.Select(r => new WaterIntensityMetric(
            WaterIntensityMetricId.New(), r.WaterConsumedCubicMeters, r.OutputUnits, r.MethodologyVersion,
            UtcTimestamp.From(r.ReadingAtUtc), UtcTimestamp.From(r.RecordedAtUtc), new UserId(r.RecordedBy))).ToList());

        var wasteRows = await queryExecutor.QueryAsync<WasteDiversionRecordRow>("[SustainabilityAdvancedMetrics].[usp_WasteDiversionRecord_ListByLedger]", listParameters, cancellationToken);
        AggregateReflectionHydrator.SetField(agg, "_wasteDiversionRecords", wasteRows.Select(r => new WasteDiversionRecord(
            WasteDiversionRecordId.New(), r.TotalWasteKg, r.DivertedWasteKg, r.MethodologyVersion,
            UtcTimestamp.From(r.IntervalEndAtUtc), UtcTimestamp.From(r.RecordedAtUtc), new UserId(r.RecordedBy))).ToList());

        var targetRows = await queryExecutor.QueryAsync<SustainabilityTargetRow>("[SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_ListByLedger]", listParameters, cancellationToken);
        var targets = new List<SustainabilityTarget>();
        foreach (var r in targetRows)
        {
            var target = new SustainabilityTarget(
                SustainabilityTargetId.New(), Enum.Parse<SustainabilityTargetType>(r.TargetType), r.BaselineValue, r.TargetValue,
                UtcTimestamp.From(r.TargetDateUtc), UtcTimestamp.From(r.SetAtUtc), new UserId(r.SetBy));

            if (r.CurrentValue != r.BaselineValue || r.Status != SustainabilityTargetStatus.OnTrack.ToString())
            {
                AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.CurrentValue), r.CurrentValue);
                AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.Status), Enum.Parse<SustainabilityTargetStatus>(r.Status));
                AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.LastProgressUpdateAtUtc), UtcTimestamp.From(r.LastProgressUpdateAtUtc ?? r.SetAtUtc));
            }

            targets.Add(target);
        }
        AggregateReflectionHydrator.SetField(agg, "_sustainabilityTargets", targets);

        return agg;
    }

    private void DispatchDomainEvents(AdvancedMetricsLedger aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private sealed record LedgerRow
    {
        public required Guid AdvancedMetricsLedgerId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required DateTime ReportingPeriodStartUtc { get; init; }
        public required DateTime ReportingPeriodEndUtc { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }

    private sealed record CarbonEmissionRecordRow
    {
        public required string Scope { get; init; }
        public required string ActivityDescription { get; init; }
        public decimal EmissionsMetricTonsCo2e { get; init; }
        public required string MethodologyVersion { get; init; }
        public required DateTime ActivityAtUtc { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    private sealed record EnergyIntensityMetricRow
    {
        public decimal EnergyConsumedKwh { get; init; }
        public decimal OutputUnits { get; init; }
        public required string MethodologyVersion { get; init; }
        public required DateTime ReadingAtUtc { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    private sealed record WaterIntensityMetricRow
    {
        public decimal WaterConsumedCubicMeters { get; init; }
        public decimal OutputUnits { get; init; }
        public required string MethodologyVersion { get; init; }
        public required DateTime ReadingAtUtc { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    private sealed record WasteDiversionRecordRow
    {
        public decimal TotalWasteKg { get; init; }
        public decimal DivertedWasteKg { get; init; }
        public required string MethodologyVersion { get; init; }
        public required DateTime IntervalEndAtUtc { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
    }

    private sealed record SustainabilityTargetRow
    {
        public required string TargetType { get; init; }
        public decimal BaselineValue { get; init; }
        public decimal TargetValue { get; init; }
        public decimal CurrentValue { get; init; }
        public required DateTime TargetDateUtc { get; init; }
        public required string Status { get; init; }
        public required DateTime SetAtUtc { get; init; }
        public required Guid SetBy { get; init; }
        public DateTime? LastProgressUpdateAtUtc { get; init; }
    }
}
