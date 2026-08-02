// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IDashboardRepository (Domain repository
//                  interface for the Dashboard aggregate root). All DB access via stored
//                  procedures in the [Analytics] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Dashboard exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Named
//                  "...AggregateDapperRepository" rather than "DashboardDapperRepository" to avoid
//                  a same-simple-name-as-namespace-segment collision with the enclosing
//                  ...Analytics.Dashboard.Persistence namespace (the same fix SafetyEvents agents
//                  applied elsewhere in this codebase). Both AddAsync AND UpdateAsync enqueue
//                  domain events to the outbox after their write. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag from day one. Phase 4 full
//                  build-out -- this aggregate previously had zero persistence at all. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Analytics.Dashboard.Aggregates;
using PlantSafety.Domain.Analytics.Dashboard.Enums;
using PlantSafety.Domain.Analytics.Dashboard.Events;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Domain.Analytics.Dashboard.Specifications;
using PlantSafety.Domain.Analytics.Dashboard.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard"/> aggregate root.</summary>
public sealed class DashboardAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DashboardAggregateDapperRepository> logger)
    : IDashboardRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard?> GetByIdAsync(DashboardId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Dashboard {DashboardId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DashboardId", id.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DashboardRow>(
            "[Analytics].[usp_Dashboard_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard>> ListAsync(DashboardSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Dashboard via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<DashboardRow>(
            "[Analytics].[usp_Dashboard_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Dashboard {DashboardId}", aggregate.DashboardId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DashboardId", aggregate.DashboardId.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("DashboardName", aggregate.DashboardName, 400)
            .AddString("DashboardType", aggregate.DashboardType, 100)
            .AddGuid("OwnerId", aggregate.OwnerId.Value == Guid.Empty ? (Guid?)null : aggregate.OwnerId.Value)
            .AddInt("RefreshIntervalSeconds", aggregate.RefreshIntervalSeconds)
            .AddInt("WidgetCount", aggregate.WidgetCount)
            .AddBool("IsPublished", aggregate.IsPublished)
            .AddDateTime("LastRefreshedAtUtc", aggregate.LastRefreshedAtUtc?.Value)
            .AddGuid("PublishedByUserId", aggregate.PublishedByUserId?.Value)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddGuid("DeprecatedByUserId", aggregate.DeprecatedByUserId?.Value)
            .AddDateTime("DeprecatedAtUtc", aggregate.DeprecatedAtUtc?.Value)
            .AddGuid("ClosedByUserId", aggregate.ClosedByUserId?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_Dashboard_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Dashboard {DashboardId}", aggregate.DashboardId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DashboardId", aggregate.DashboardId.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("DashboardName", aggregate.DashboardName, 400)
            .AddString("DashboardType", aggregate.DashboardType, 100)
            .AddGuid("OwnerId", aggregate.OwnerId.Value == Guid.Empty ? (Guid?)null : aggregate.OwnerId.Value)
            .AddInt("RefreshIntervalSeconds", aggregate.RefreshIntervalSeconds)
            .AddInt("WidgetCount", aggregate.WidgetCount)
            .AddBool("IsPublished", aggregate.IsPublished)
            .AddDateTime("LastRefreshedAtUtc", aggregate.LastRefreshedAtUtc?.Value)
            .AddGuid("PublishedByUserId", aggregate.PublishedByUserId?.Value)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddGuid("DeprecatedByUserId", aggregate.DeprecatedByUserId?.Value)
            .AddDateTime("DeprecatedAtUtc", aggregate.DeprecatedAtUtc?.Value)
            .AddGuid("ClosedByUserId", aggregate.ClosedByUserId?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_Dashboard_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Analytics].[usp_Dashboard_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Dashboard {aggregate.DashboardId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="Dashboard"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard Hydrate(DashboardRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.DashboardId), DashboardId.FromGuid(row.DashboardId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.Status), Enum.Parse<DashboardStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.DashboardName), row.DashboardName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.DashboardType), row.DashboardType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.OwnerId), row.OwnerId.HasValue ? new UserId(row.OwnerId.Value) : default);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.RefreshIntervalSeconds), row.RefreshIntervalSeconds);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.WidgetCount), row.WidgetCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.IsPublished), row.IsPublished);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.LastRefreshedAtUtc), row.LastRefreshedAtUtc.HasValue ? UtcTimestamp.From(row.LastRefreshedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.PublishedByUserId), row.PublishedByUserId.HasValue ? new UserId(row.PublishedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.DeprecatedByUserId), row.DeprecatedByUserId.HasValue ? new UserId(row.DeprecatedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.DeprecatedAtUtc), row.DeprecatedAtUtc.HasValue ? UtcTimestamp.From(row.DeprecatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.ClosedByUserId), row.ClosedByUserId.HasValue ? new UserId(row.ClosedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IDashboardDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Dashboard_GetById</c>/<c>usp_Dashboard_GetAllActive</c>.</summary>
    private sealed record DashboardRow
    {
        /// <summary>Optimistic-concurrency token, not consumed by aggregate hydration.</summary>
        public string? ETag { get; init; }

        public required Guid DashboardId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DashboardName { get; init; }
        public required string DashboardType { get; init; }
        public Guid? OwnerId { get; init; }
        public int RefreshIntervalSeconds { get; init; }
        public int WidgetCount { get; init; }
        public bool IsPublished { get; init; }
        public DateTime? LastRefreshedAtUtc { get; init; }
        public Guid? PublishedByUserId { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public Guid? DeprecatedByUserId { get; init; }
        public DateTime? DeprecatedAtUtc { get; init; }
        public Guid? ClosedByUserId { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
