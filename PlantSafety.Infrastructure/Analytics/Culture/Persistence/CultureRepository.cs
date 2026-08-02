// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Culture aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added GetByIdAsync (aggregate hydration,
//                  tenant-scoped) and UpdateAsync (optimistic-concurrency ETag), backing the new
//                  Review/Publish/Close/Archive lifecycle transitions. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write -- the same
//                  platform-wide convention as PermitsDapperRepository/AccidentDapperRepository.
//                  Uses AggregateReflectionHydrator because Culture exposes only a private "for
//                  rehydration only" constructor and private-setter properties, mirroring
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Analytics.Culture.Enums;
using PlantSafety.Domain.Analytics.Culture.Events;
using PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using ICultureRepository = PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper-backed repository for <see cref="CultureReadModel"/> queries and the <see cref="CultureAggregate"/> write side.</summary>
public sealed class CultureRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CultureRepository> logger) : ICultureRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(CultureAggregate culture, CancellationToken cancellationToken)
    {
        logger.LogDebug("Add Culture {CultureId}", culture.CultureId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CultureId", culture.CultureId.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), culture.TenantId)
            .AddGuid("SiteId", culture.SiteId)
            .AddString("Status", culture.Status.ToString(), 30)
            .AddString("SurveyPeriod", culture.SurveyPeriod, 20)
            .AddDecimal("CultureIndexScore", culture.CultureIndexScore)
            .AddDecimal("EngagementScore", culture.EngagementScore)
            .AddDecimal("ParticipationRate", culture.ParticipationRate)
            .AddString("TopRiskTheme", culture.TopRiskTheme, 200)
            .AddDateTime("CreatedAtUtc", culture.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", culture.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_Culture_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in culture.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, culture.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        culture.ClearDomainEvents();
    }

    /// <summary>Retrieves a Culture aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    public async Task<CultureAggregate?> GetByIdAsync(CultureId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById (aggregate) for Culture {CultureId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("CultureId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CultureReadModel>(
            "[Analytics].[usp_Culture_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <summary>
    /// Persists changes to an existing Culture aggregate, enforcing optimistic concurrency.
    /// </summary>
    public async Task UpdateAsync(CultureAggregate culture, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Culture {CultureId}", culture.CultureId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CultureId", culture.CultureId.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), culture.TenantId)
            .AddString("Status", culture.Status.ToString(), 30)
            .AddString("SurveyPeriod", culture.SurveyPeriod, 20)
            .AddDecimal("CultureIndexScore", culture.CultureIndexScore)
            .AddDecimal("EngagementScore", culture.EngagementScore)
            .AddDecimal("ParticipationRate", culture.ParticipationRate)
            .AddString("TopRiskTheme", culture.TopRiskTheme, 200)
            .AddGuid("ReviewedByUserId", culture.ReviewedByUserId?.Value)
            .AddDateTime("ReviewedAtUtc", culture.ReviewedAtUtc?.Value)
            .AddGuid("PublishedByUserId", culture.PublishedByUserId?.Value)
            .AddDateTime("PublishedAtUtc", culture.PublishedAtUtc?.Value)
            .AddGuid("ClosedByUserId", culture.ClosedByUserId?.Value)
            .AddDateTime("ClosedAtUtc", culture.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", culture.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", culture.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_Culture_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Analytics].[usp_Culture_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Culture {culture.CultureId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in culture.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, culture.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        culture.ClearDomainEvents();
    }

    /// <summary>Returns the Culture read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<CultureReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Culture {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("CultureId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetById]", parameters);
    }

    /// <summary>Returns all Culture read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<CultureReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Culture tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }

    /// <summary>Reconstructs a <see cref="CultureAggregate"/> from its flat read-model row.</summary>
    private static CultureAggregate Hydrate(CultureReadModel row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CultureAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.CultureId), CultureId.FromGuid(row.CultureId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.Status), Enum.Parse<CultureStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.SurveyPeriod), row.SurveyPeriod);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.CultureIndexScore), row.CultureIndexScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.EngagementScore), row.EngagementScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ParticipationRate), row.ParticipationRate);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.TopRiskTheme), row.TopRiskTheme);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ReviewedByUserId), row.ReviewedByUserId.HasValue ? new UserId(row.ReviewedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ReviewedAtUtc), row.ReviewedAtUtc.HasValue ? UtcTimestamp.From(row.ReviewedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.PublishedByUserId), row.PublishedByUserId.HasValue ? new UserId(row.PublishedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ClosedByUserId), row.ClosedByUserId.HasValue ? new UserId(row.ClosedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CultureAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICultureDomainEvent>());

        return agg;
    }
}
