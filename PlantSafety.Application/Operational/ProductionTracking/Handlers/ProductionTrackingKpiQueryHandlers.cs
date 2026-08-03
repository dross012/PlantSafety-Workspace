// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionTrackingKpiQueryHandlers.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    The four ProductionTracking queries that compose real Phase 5 infrastructure
//                  rather than reimplementing KPI/anomaly math: GetThroughputSummaryQuery and
//                  GetBottleneckSummaryQuery reuse
//                  Observability.Telemetry.TelemetryAnomalyDetector/ProductionTelemetryStreamProcessor;
//                  GetRunKpisQuery and GetPlantKpisQuery reuse
//                  Analytics.Dashboard.Kpi.ProductionKpiEngine. Per docs/Phase5/ui-gaps-scoping.md's
//                  "ProductionTracking" suggested approach: "the sane path is: define ProductionRun/
//                  Batch as read-model projections... coordinate with whoever owns
//                  Observability/Telemetry" -- these handlers are the concrete coordination point.
//                  KPI targets (OEE 85%, Yield 98%, Scrap Rate 2%, MTBF 720h, MTTR 2h) and the
//                  baseline scrap rate (2%) used for quality-drift detection are platform-standard
//                  manufacturing benchmarks, documented here as explicit assumptions pending a future
//                  per-line/per-product configuration surface. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Analytics.Dashboard.Kpi;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.Telemetry;
using PlantSafety.Application.Operational.ProductionTracking.Models;
using PlantSafety.Application.Operational.ProductionTracking.Queries;
using PlantSafety.Application.Operational.ProductionTracking.Repositories;
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;
using PlantSafety.Domain.Observability.Telemetry.Services;
using PlantSafety.Domain.Observability.Telemetry.ValueObjects;

namespace PlantSafety.Application.Operational.ProductionTracking.Handlers;

/// <summary>Handles <see cref="GetThroughputSummaryQuery"/> by aggregating active runs' real telemetry-derived throughput via <see cref="ThroughputCalculator"/>-equivalent math.</summary>
public sealed class GetThroughputSummaryQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetThroughputSummaryQueryHandler> logger)
    : IRequestHandler<GetThroughputSummaryQuery, Result<ThroughputSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<ThroughputSummaryDto>> Handle(GetThroughputSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var inputs = await repository.GetActiveRunKpiInputsAsync(request.TenantId, cancellationToken);
            var now = DateTime.UtcNow;

            double currentThroughput = 0.0;
            double targetThroughput = 0.0;

            foreach (var run in inputs)
            {
                var elapsed = ProductionKpiInputMapper.ElapsedSince(run, now);
                var totalUnits = run.GoodUnitCount + run.ScrapUnitCount;
                if (elapsed > TimeSpan.Zero)
                {
                    currentThroughput += totalUnits / elapsed.TotalHours;
                }
                if (run.IdealCycleTimeSeconds > 0)
                {
                    targetThroughput += 3600.0 / run.IdealCycleTimeSeconds;
                }
            }

            var efficiencyPercent = targetThroughput > 0 ? Math.Round(currentThroughput / targetThroughput * 100.0, 2) : 0.0;

            return Result<ThroughputSummaryDto>.Success(new ThroughputSummaryDto
            {
                CurrentThroughput = Math.Round(currentThroughput, 2),
                TargetThroughput = Math.Round(targetThroughput, 2),
                EfficiencyPercent = efficiencyPercent,
                // No historical trend data source exists yet for throughput -- documented assumption,
                // mirrors the platform-wide precedent (e.g. Maintenance's HasConflict=false) of a safe,
                // conservative constant where no real trend computation exists.
                TrendDirection = "Stable",
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute throughput summary for Tenant {TenantId}", request.TenantId);
            return Result<ThroughputSummaryDto>.Failure(new ApplicationError(
                "Operational.ProductionTracking.ThroughputSummaryFailed", "Unable to retrieve the throughput summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetBottleneckSummaryQuery"/> by reusing the real <see cref="TelemetryAnomalyDetector"/> via <see cref="ProductionTelemetryStreamProcessor"/>.</summary>
public sealed class GetBottleneckSummaryQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetBottleneckSummaryQueryHandler> logger)
    : IRequestHandler<GetBottleneckSummaryQuery, Result<BottleneckSummaryDto>>
{
    private readonly ProductionTelemetryStreamProcessor _streamProcessor = new();

    /// <inheritdoc/>
    public async Task<Result<BottleneckSummaryDto>> Handle(GetBottleneckSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var inputs = await repository.GetActiveRunKpiInputsAsync(request.TenantId, cancellationToken);
            var batches = inputs.Select(ProductionKpiInputMapper.ToTelemetryBatch).ToList();

            var anomalies = _streamProcessor.ProcessBatch(batches);

            var severity = anomalies.Count == 0
                ? "None"
                : anomalies.Max(a => a.DeviationRatio) switch
                {
                    double.PositiveInfinity => "High",
                    var d when d >= 2.0 => "High",
                    var d when d >= 1.5 => "Medium",
                    _ => "Low",
                };

            return Result<BottleneckSummaryDto>.Success(new BottleneckSummaryDto
            {
                ActiveBottlenecks = anomalies.Count,
                Severity = severity,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute bottleneck summary for Tenant {TenantId}", request.TenantId);
            return Result<BottleneckSummaryDto>.Failure(new ApplicationError(
                "Operational.ProductionTracking.BottleneckSummaryFailed", "Unable to retrieve the bottleneck summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunKpisQuery"/> by composing the real <see cref="ProductionKpiEngine"/>.</summary>
public sealed class GetRunKpisQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunKpisQueryHandler> logger)
    : IRequestHandler<GetRunKpisQuery, Result<PaginatedResult<KpiTileDto>>>
{
    private readonly ProductionKpiEngine _kpiEngine = new();

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<KpiTileDto>>> Handle(GetRunKpisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var input = await repository.GetRunKpiInputAsync(request.TenantId, request.RunId, cancellationToken);
            if (input is null)
            {
                return Result<PaginatedResult<KpiTileDto>>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound", "The requested production run could not be found.", request.RunId, ErrorSeverity.NotFound));
            }

            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var snapshot = ProductionKpiInputMapper.ComputeSnapshot(_kpiEngine, input, DateTime.UtcNow);
            var targetThroughput = input.IdealCycleTimeSeconds > 0 ? 3600.0 / input.IdealCycleTimeSeconds : 0.0;
            var tiles = ProductionKpiInputMapper.ToKpiTiles(snapshot, targetThroughput);

            return Result<PaginatedResult<KpiTileDto>>.Success(new PaginatedResult<KpiTileDto>(tiles, tiles.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute KPIs for ProductionRun {RunId}", request.RunId);
            return Result<PaginatedResult<KpiTileDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunKpisFailed", "Unable to retrieve the production run's KPIs at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetPlantKpisQuery"/> by composing the real <see cref="ProductionKpiEngine"/> over every active run.</summary>
public sealed class GetPlantKpisQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetPlantKpisQueryHandler> logger)
    : IRequestHandler<GetPlantKpisQuery, Result<PaginatedResult<KpiTileDto>>>
{
    private readonly ProductionKpiEngine _kpiEngine = new();

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<KpiTileDto>>> Handle(GetPlantKpisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var inputs = await repository.GetActiveRunKpiInputsAsync(request.TenantId, cancellationToken);
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            if (inputs.Count == 0)
            {
                var emptyTiles = ProductionKpiInputMapper.EmptyKpiTiles();
                return Result<PaginatedResult<KpiTileDto>>.Success(new PaginatedResult<KpiTileDto>(emptyTiles, emptyTiles.Count, pageNumber, pageSize));
            }

            var aggregate = ProductionKpiInputMapper.AggregateAcrossRuns(inputs, DateTime.UtcNow);
            var snapshot = ProductionKpiInputMapper.ComputeSnapshotFromAggregate(_kpiEngine, aggregate);
            var tiles = ProductionKpiInputMapper.ToKpiTiles(snapshot, aggregate.TargetThroughputUnitsPerHour);

            return Result<PaginatedResult<KpiTileDto>>.Success(new PaginatedResult<KpiTileDto>(tiles, tiles.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute plant-wide KPIs for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<KpiTileDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.PlantKpisFailed", "Unable to retrieve the plant-wide KPIs at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>
/// Internal aggregate KPI-input shape spanning every currently active ProductionRun, used only by
/// <see cref="GetPlantKpisQueryHandler"/>.
/// </summary>
internal sealed record AggregatedKpiInput(
    TimeSpan TotalOperatingTime,
    TimeSpan TotalDowntime,
    double AverageIdealCycleTimeSeconds,
    int TotalUnitsProduced,
    int TotalGoodUnits,
    int TotalScrapUnits,
    int TotalFailureCount,
    TimeSpan TotalRepairTime,
    double TargetThroughputUnitsPerHour);

/// <summary>
/// Maps persisted <see cref="ProductionRunKpiInputDto"/> read-model rows onto the real Phase 5 KPI
/// engine's/telemetry anomaly detector's input value objects. Pure mapping -- no KPI/anomaly math
/// lives here; it all delegates to <see cref="ProductionKpiEngine"/>/<see cref="TelemetryAnomalyDetector"/>.
/// </summary>
internal static class ProductionKpiInputMapper
{
    /// <summary>Platform-standard OEE target (world-class benchmark), pending per-line configuration.</summary>
    public const double OeeTarget = 0.85;

    /// <summary>Platform-standard Yield target, pending per-product configuration.</summary>
    public const double YieldTarget = 0.98;

    /// <summary>Platform-standard Scrap Rate target, pending per-product configuration.</summary>
    public const double ScrapRateTarget = 0.02;

    /// <summary>Platform-standard MTBF target (30 days), pending per-asset-class configuration.</summary>
    public static readonly TimeSpan MtbfTarget = TimeSpan.FromDays(30);

    /// <summary>Platform-standard MTTR target, pending per-asset-class configuration.</summary>
    public static readonly TimeSpan MttrTarget = TimeSpan.FromHours(2);

    /// <summary>Baseline scrap rate used for quality-drift anomaly detection, pending per-product configuration.</summary>
    public const double BaselineScrapRate = 0.02;

    /// <summary>Returns the elapsed duration of a run, treating an active (unended) run as elapsed-to-now.</summary>
    public static TimeSpan ElapsedSince(ProductionRunKpiInputDto run, DateTime nowUtc)
    {
        var end = run.EndedAtUtc ?? nowUtc;
        var elapsed = end - run.StartedAtUtc;
        return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.FromSeconds(1);
    }

    /// <summary>Converts a single run's inputs into a <see cref="MachineTelemetryBatch"/> for the real telemetry stream processor.</summary>
    public static MachineTelemetryBatch ToTelemetryBatch(ProductionRunKpiInputDto run)
    {
        var totalUnits = run.GoodUnitCount + run.ScrapUnitCount;
        var elapsed = ElapsedSince(run, DateTime.UtcNow);
        var observedCycleTime = totalUnits > 0
            ? TimeSpan.FromSeconds(elapsed.TotalSeconds / totalUnits)
            : TimeSpan.FromSeconds(run.IdealCycleTimeSeconds);

        return new MachineTelemetryBatch
        {
            MachineId = run.MachineId,
            LatestCycleTimeReading = new CycleTimeReading
            {
                MachineId = run.MachineId,
                ObservedCycleTime = observedCycleTime,
                IdealCycleTime = TimeSpan.FromSeconds(run.IdealCycleTimeSeconds),
                ObservedAtUtc = UtcTimestamp.Now(),
            },
            DowntimeEvents = run.DowntimeEvents.Select(d => new DowntimeEvent
            {
                MachineId = run.MachineId,
                ReasonCode = d.ReasonCode,
                StartUtc = UtcTimestamp.From(d.StartUtc),
                EndUtc = d.EndUtc.HasValue ? UtcTimestamp.From(d.EndUtc.Value) : null,
                IsPlanned = d.IsPlanned,
            }).ToList(),
            ProductionCountReading = new ProductionCountReading
            {
                MachineId = run.MachineId,
                WindowStartUtc = UtcTimestamp.From(run.StartedAtUtc),
                WindowEndUtc = UtcTimestamp.From(run.EndedAtUtc ?? DateTime.UtcNow),
                GoodUnitCount = run.GoodUnitCount,
                ScrapUnitCount = run.ScrapUnitCount,
            },
            BaselineScrapRate = BaselineScrapRate,
        };
    }

    /// <summary>Builds the five KPI engine input VOs for a single run and computes its <see cref="ProductionKpiSnapshot"/>.</summary>
    public static ProductionKpiSnapshot ComputeSnapshot(ProductionKpiEngine engine, ProductionRunKpiInputDto run, DateTime nowUtc)
    {
        var elapsed = ElapsedSince(run, nowUtc);
        var downtime = ClampDowntime(TimeSpan.FromMinutes(run.DowntimeMinutesTotal), elapsed);
        var totalUnits = run.GoodUnitCount + run.ScrapUnitCount;
        var unplannedEvents = run.DowntimeEvents.Where(d => !d.IsPlanned).ToList();
        var unplannedMinutes = unplannedEvents.Sum(d => (d.EndUtc ?? nowUtc).Subtract(d.StartUtc).TotalMinutes);

        var oeeInputs = new OeeInputs(
            plannedProductionTime: elapsed,
            downtimeDuration: downtime,
            idealCycleTime: TimeSpan.FromSeconds(run.IdealCycleTimeSeconds),
            totalUnitsProduced: totalUnits,
            goodUnitsProduced: run.GoodUnitCount);

        var mtbfInputs = new MtbfInputs(
            totalOperatingTime: elapsed - downtime,
            failureCount: unplannedEvents.Count);

        var mttrInputs = new MttrInputs(
            totalRepairTime: TimeSpan.FromMinutes(Math.Max(unplannedMinutes, 0)),
            repairCount: unplannedEvents.Count);

        var throughputInputs = new ThroughputInputs(
            unitsProduced: totalUnits,
            elapsedTime: elapsed);

        var qualityInputs = new ProductionQualityInputs(
            totalUnitsProduced: totalUnits,
            goodUnitsProduced: run.GoodUnitCount,
            scrappedUnits: run.ScrapUnitCount);

        return engine.ComputeSnapshot(oeeInputs, mtbfInputs, mttrInputs, throughputInputs, qualityInputs);
    }

    /// <summary>Aggregates KPI inputs across every currently active ProductionRun for the plant-wide KPI dashboard.</summary>
    public static AggregatedKpiInput AggregateAcrossRuns(IReadOnlyList<ProductionRunKpiInputDto> runs, DateTime nowUtc)
    {
        var totalOperating = TimeSpan.Zero;
        var totalDowntime = TimeSpan.Zero;
        var totalRepair = TimeSpan.Zero;
        int totalGood = 0, totalScrap = 0, totalFailures = 0;
        double targetThroughput = 0.0;
        double idealCycleSum = 0.0;

        foreach (var run in runs)
        {
            var elapsed = ElapsedSince(run, nowUtc);
            var downtime = ClampDowntime(TimeSpan.FromMinutes(run.DowntimeMinutesTotal), elapsed);
            var unplanned = run.DowntimeEvents.Where(d => !d.IsPlanned).ToList();

            totalOperating += elapsed - downtime;
            totalDowntime += downtime;
            totalGood += run.GoodUnitCount;
            totalScrap += run.ScrapUnitCount;
            totalFailures += unplanned.Count;
            totalRepair += TimeSpan.FromMinutes(Math.Max(unplanned.Sum(d => (d.EndUtc ?? nowUtc).Subtract(d.StartUtc).TotalMinutes), 0));
            idealCycleSum += run.IdealCycleTimeSeconds;
            if (run.IdealCycleTimeSeconds > 0)
                targetThroughput += 3600.0 / run.IdealCycleTimeSeconds;
        }

        var averageIdealCycle = runs.Count > 0 ? idealCycleSum / runs.Count : 0.0;

        return new AggregatedKpiInput(
            TotalOperatingTime: totalOperating,
            TotalDowntime: totalDowntime,
            AverageIdealCycleTimeSeconds: averageIdealCycle,
            TotalUnitsProduced: totalGood + totalScrap,
            TotalGoodUnits: totalGood,
            TotalScrapUnits: totalScrap,
            TotalFailureCount: totalFailures,
            TotalRepairTime: totalRepair,
            TargetThroughputUnitsPerHour: targetThroughput);
    }

    /// <summary>Computes the plant-wide <see cref="ProductionKpiSnapshot"/> from a pre-aggregated fleet input.</summary>
    public static ProductionKpiSnapshot ComputeSnapshotFromAggregate(ProductionKpiEngine engine, AggregatedKpiInput aggregate)
    {
        var plannedProductionTime = aggregate.TotalOperatingTime + aggregate.TotalDowntime;
        if (plannedProductionTime <= TimeSpan.Zero)
            plannedProductionTime = TimeSpan.FromSeconds(1);

        var idealCycleTime = aggregate.AverageIdealCycleTimeSeconds > 0
            ? TimeSpan.FromSeconds(aggregate.AverageIdealCycleTimeSeconds)
            : TimeSpan.FromSeconds(1);

        var oeeInputs = new OeeInputs(
            plannedProductionTime: plannedProductionTime,
            downtimeDuration: aggregate.TotalDowntime > plannedProductionTime ? plannedProductionTime : aggregate.TotalDowntime,
            idealCycleTime: idealCycleTime,
            totalUnitsProduced: aggregate.TotalUnitsProduced,
            goodUnitsProduced: aggregate.TotalGoodUnits);

        var mtbfInputs = new MtbfInputs(aggregate.TotalOperatingTime, aggregate.TotalFailureCount);
        var mttrInputs = new MttrInputs(aggregate.TotalRepairTime, aggregate.TotalFailureCount);
        var throughputInputs = new ThroughputInputs(aggregate.TotalUnitsProduced, plannedProductionTime);
        var qualityInputs = new ProductionQualityInputs(aggregate.TotalUnitsProduced, aggregate.TotalGoodUnits, aggregate.TotalScrapUnits);

        return engine.ComputeSnapshot(oeeInputs, mtbfInputs, mttrInputs, throughputInputs, qualityInputs);
    }

    /// <summary>Maps a computed <see cref="ProductionKpiSnapshot"/> onto the UI-facing <see cref="KpiTileDto"/> tile set.</summary>
    public static List<KpiTileDto> ToKpiTiles(ProductionKpiSnapshot snapshot, double targetThroughput) =>
    [
        new KpiTileDto { MetricName = "OEE", Value = Math.Round(snapshot.Oee.Oee * 100.0, 2), Target = OeeTarget * 100.0, Unit = "%", Status = EvaluateStatus(snapshot.Oee.Oee, OeeTarget, higherIsBetter: true) },
        new KpiTileDto { MetricName = "Throughput", Value = Math.Round(snapshot.ThroughputUnitsPerHour, 2), Target = Math.Round(targetThroughput, 2), Unit = "units/hr", Status = EvaluateStatus(snapshot.ThroughputUnitsPerHour, targetThroughput, higherIsBetter: true) },
        new KpiTileDto { MetricName = "Yield", Value = Math.Round(snapshot.Yield * 100.0, 2), Target = YieldTarget * 100.0, Unit = "%", Status = EvaluateStatus(snapshot.Yield, YieldTarget, higherIsBetter: true) },
        new KpiTileDto { MetricName = "Scrap Rate", Value = Math.Round(snapshot.ScrapRate * 100.0, 2), Target = ScrapRateTarget * 100.0, Unit = "%", Status = EvaluateStatus(snapshot.ScrapRate, ScrapRateTarget, higherIsBetter: false) },
        new KpiTileDto { MetricName = "MTBF", Value = Math.Round(snapshot.Mtbf.TotalHours, 2), Target = Math.Round(MtbfTarget.TotalHours, 2), Unit = "hours", Status = EvaluateStatus(snapshot.Mtbf.TotalHours, MtbfTarget.TotalHours, higherIsBetter: true) },
        new KpiTileDto { MetricName = "MTTR", Value = Math.Round(snapshot.Mttr.TotalHours, 2), Target = Math.Round(MttrTarget.TotalHours, 2), Unit = "hours", Status = EvaluateStatus(snapshot.Mttr.TotalHours, MttrTarget.TotalHours, higherIsBetter: false) },
    ];

    /// <summary>Returns a zero-valued tile set for when no active runs exist (avoids a divide-by-zero KPI snapshot).</summary>
    public static List<KpiTileDto> EmptyKpiTiles() =>
    [
        new KpiTileDto { MetricName = "OEE", Value = 0.0, Target = OeeTarget * 100.0, Unit = "%", Status = "Missed" },
        new KpiTileDto { MetricName = "Throughput", Value = 0.0, Target = 0.0, Unit = "units/hr", Status = "Missed" },
        new KpiTileDto { MetricName = "Yield", Value = 0.0, Target = YieldTarget * 100.0, Unit = "%", Status = "Missed" },
        new KpiTileDto { MetricName = "Scrap Rate", Value = 0.0, Target = ScrapRateTarget * 100.0, Unit = "%", Status = "Met" },
        new KpiTileDto { MetricName = "MTBF", Value = 0.0, Target = Math.Round(MtbfTarget.TotalHours, 2), Unit = "hours", Status = "Missed" },
        new KpiTileDto { MetricName = "MTTR", Value = 0.0, Target = Math.Round(MttrTarget.TotalHours, 2), Unit = "hours", Status = "Met" },
    ];

    private static TimeSpan ClampDowntime(TimeSpan downtime, TimeSpan plannedProductionTime) =>
        downtime > plannedProductionTime ? plannedProductionTime : downtime;

    private static string EvaluateStatus(double value, double target, bool higherIsBetter)
    {
        if (higherIsBetter)
        {
            if (target <= 0) return value > 0 ? "Met" : "Warning";
            if (value >= target) return "Met";
            if (value >= target * 0.9) return "Warning";
            return "Missed";
        }

        if (target <= 0) return value <= 0 ? "Met" : "Missed";
        if (value <= target) return "Met";
        if (value <= target * 1.1) return "Warning";
        return "Missed";
    }
}
