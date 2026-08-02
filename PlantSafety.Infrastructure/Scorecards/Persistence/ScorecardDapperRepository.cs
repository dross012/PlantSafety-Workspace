// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Infrastructure
//  File:           ScorecardDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IScorecardRepository (Domain repository
//                  interface for the Scorecard aggregate root). All DB access via stored
//                  procedures in the [Scorecards] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Scorecard exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied ScorecardSpecification in-memory over the full active
//                  candidate set, since the Domain-layer specification is an
//                  Expression&lt;Func&lt;Scorecard,bool&gt;&gt; rather than a SQL predicate, and the
//                  interface carries no tenant/site scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scorecards.Aggregates;
using PlantSafety.Domain.Scorecards.Entities;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.Events;
using PlantSafety.Domain.Scorecards.Interfaces;
using PlantSafety.Domain.Scorecards.Specifications;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scorecards.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Scorecard"/> aggregate root.</summary>
public sealed class ScorecardDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ScorecardDapperRepository> logger)
    : IScorecardRepository
{
    /// <inheritdoc/>
    public async Task<Scorecard?> GetByIdAsync(ScorecardId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Scorecard {ScorecardId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("ScorecardId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ScorecardRow>(
            "[Scorecards].[usp_Scorecard_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var kpiRows = await GetKpiMeasurementRowsAsync(id, cancellationToken);
        return Hydrate(row, kpiRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Scorecard>> ListAsync(ScorecardSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Scorecard via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<ScorecardRow>(
            "[Scorecards].[usp_Scorecard_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<Scorecard>();

        foreach (var row in rows)
        {
            var kpiRows = await GetKpiMeasurementRowsAsync(ScorecardId.FromGuid(row.ScorecardId), cancellationToken);
            var aggregate = Hydrate(row, kpiRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(Scorecard aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Scorecard {ScorecardId}", aggregate.ScorecardId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScorecardId", aggregate.ScorecardId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("ScorecardTitle", aggregate.ScorecardTitle, 400)
            .AddInt("ScorecardVersion", aggregate.ScorecardVersion)
            .AddString("Category", aggregate.Category.Code, 30)
            .AddString("PeriodType", aggregate.PeriodType.Code, 30)
            .AddDateTime("PeriodStartUtc", aggregate.PeriodStartUtc.Value)
            .AddDateTime("PeriodEndUtc", aggregate.PeriodEndUtc.Value)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddInt("DistributionCount", aggregate.DistributionCount)
            .AddBool("ReviewRequired", aggregate.ReviewRequired)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scorecards].[usp_Scorecard_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc/>
    public async Task UpdateAsync(Scorecard aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Scorecard {ScorecardId}", aggregate.ScorecardId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScorecardId", aggregate.ScorecardId.Value)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddInt("DistributionCount", aggregate.DistributionCount)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scorecards].[usp_Scorecard_Update]", parameters, tx, cancellationToken: cancellationToken);

        // Reconcile Measurements: matched by (ScorecardId, Metric) since RecordMetric() replaces
        // any prior measurement for the same metric code with a brand-new KpiMeasurement.
        foreach (var kpi in aggregate.Measurements)
        {
            var kpiParams = new StoredProcedureBuilder()
                .AddGuid("KpiMeasurementId", kpi.KpiMeasurementId.Value)
                .AddGuid("ScorecardId", aggregate.ScorecardId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Metric", kpi.Metric.Code, 50)
                .AddDecimal("Numerator", kpi.Numerator)
                .AddDecimal("Denominator", kpi.Denominator)
                .AddDecimal("ComputedValue", kpi.ComputedValue)
                .AddString("DataSourceModule", kpi.DataSourceModule, 200)
                .AddDateTime("RecordedAtUtc", kpi.RecordedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Scorecards].[usp_KpiMeasurement_Upsert]", kpiParams, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private async Task<IReadOnlyList<KpiMeasurementRow>> GetKpiMeasurementRowsAsync(ScorecardId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ScorecardId", id.Value).Build();
        return await queryExecutor.QueryAsync<KpiMeasurementRow>(
            "[Scorecards].[usp_KpiMeasurement_GetByScorecardId]", parameters, cancellationToken);
    }

    /// <summary>Reconstructs a <see cref="Scorecard"/> aggregate from its flat read-model row and child rows.</summary>
    private static Scorecard Hydrate(ScorecardRow row, IReadOnlyList<KpiMeasurementRow> kpiRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<Scorecard>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.ScorecardId), ScorecardId.FromGuid(row.ScorecardId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.Status), Enum.Parse<ScorecardStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.ScorecardTitle), row.ScorecardTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.ScorecardVersion), row.ScorecardVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.Category), IndicatorCategory.From(row.Category));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.PeriodType), ScorecardPeriodType.From(row.PeriodType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.PeriodStartUtc), UtcTimestamp.From(row.PeriodStartUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.PeriodEndUtc), UtcTimestamp.From(row.PeriodEndUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.DistributionCount), row.DistributionCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.ReviewRequired), row.ReviewRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.ReviewCount), row.ReviewCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Scorecard.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IScorecardDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_measurements", new List<KpiMeasurement>());

        var kpiList = AggregateReflectionHydrator.GetPrivateCollectionField<Scorecard, List<KpiMeasurement>>(agg, "_measurements");
        foreach (var k in kpiRows)
        {
            var child = AggregateReflectionHydrator.CreateUninitialized<KpiMeasurement>();
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.KpiMeasurementId), KpiMeasurementId.FromGuid(k.KpiMeasurementId));
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.ScorecardId), ScorecardId.FromGuid(k.ScorecardId));
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.Metric), KpiMetricDefinition.From(k.Metric));
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.Numerator), k.Numerator);
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.Denominator), k.Denominator);
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.ComputedValue), k.ComputedValue);
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.DataSourceModule), k.DataSourceModule);
            AggregateReflectionHydrator.SetProperty(child, nameof(KpiMeasurement.RecordedAtUtc), UtcTimestamp.From(k.RecordedAtUtc));

            kpiList.Add(child);
        }

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Scorecard_GetById</c>/<c>usp_Scorecard_GetAllActive</c>.</summary>
    private sealed record ScorecardRow
    {
        public required Guid ScorecardId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ScorecardTitle { get; init; }
        public required int ScorecardVersion { get; init; }
        public required string Category { get; init; }
        public required string PeriodType { get; init; }
        public required DateTime PeriodStartUtc { get; init; }
        public required DateTime PeriodEndUtc { get; init; }
        public required Guid OwnerId { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public required int DistributionCount { get; init; }
        public required bool ReviewRequired { get; init; }
        public required int ReviewCount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }

    /// <summary>Flat read-model row shape returned by <c>usp_KpiMeasurement_GetByScorecardId</c>.</summary>
    private sealed record KpiMeasurementRow
    {
        public required Guid KpiMeasurementId { get; init; }
        public required Guid ScorecardId { get; init; }
        public required string Metric { get; init; }
        public required decimal Numerator { get; init; }
        public required decimal Denominator { get; init; }
        public required decimal ComputedValue { get; init; }
        public required string DataSourceModule { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
    }
}
