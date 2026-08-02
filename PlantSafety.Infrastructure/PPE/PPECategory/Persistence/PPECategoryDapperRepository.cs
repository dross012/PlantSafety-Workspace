// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Infrastructure
//  File:           PPECategoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPECategoryRepository (Domain repository
//                  interface for the PPECategory aggregate root). All DB access via stored
//                  procedures in the [PPE] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPECategory exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PPE.PPECategory.Aggregates;
using PlantSafety.Domain.PPE.PPECategory.Enums;
using PlantSafety.Domain.PPE.PPECategory.Events;
using PlantSafety.Domain.PPE.PPECategory.Interfaces;
using PlantSafety.Domain.PPE.PPECategory.Specifications;
using PlantSafety.Domain.PPE.PPECategory.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PPE.PPECategory.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPECategory"/> aggregate root.</summary>
public sealed class PPECategoryDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPECategoryDapperRepository> logger)
    : IPPECategoryRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory?> GetByIdAsync(PPECategoryId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPECategory {PPECategoryId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPECategoryId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPECategoryRow>(
            "[PPE].[usp_PPECategory_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory>> ListAsync(PPECategorySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPECategory via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPECategoryRow>(
            "[PPE].[usp_PPECategory_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPECategory {PPECategoryId}", aggregate.PPECategoryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPECategoryId", aggregate.PPECategoryId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("CategoryCode", aggregate.CategoryCode, 400)
            .AddString("HazardsMitigated", aggregate.HazardsMitigated, 400)
            .AddInt("InspectionIntervalDays", aggregate.InspectionIntervalDays)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPECategory_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPECategory {PPECategoryId}", aggregate.PPECategoryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPECategoryId", aggregate.PPECategoryId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("CategoryCode", aggregate.CategoryCode, 400)
            .AddString("HazardsMitigated", aggregate.HazardsMitigated, 400)
            .AddInt("InspectionIntervalDays", aggregate.InspectionIntervalDays)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPECategory_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PPECategory"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory Hydrate(PPECategoryRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory>();

        AggregateReflectionHydrator.SetProperty(agg, "PPECategoryId", PPECategoryId.FromGuid(row.PPECategoryId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPECategoryStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "CategoryCode", row.CategoryCode);
        AggregateReflectionHydrator.SetProperty(agg, "HazardsMitigated", row.HazardsMitigated);
        AggregateReflectionHydrator.SetProperty(agg, "InspectionIntervalDays", row.InspectionIntervalDays);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPECategoryDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPECategory_GetById</c>/<c>usp_PPECategory_GetAllActive</c>.</summary>
    private sealed record PPECategoryRow
    {
        public required Guid PPECategoryId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string CategoryCode { get; init; }
        public required string HazardsMitigated { get; init; }
        public required int InspectionIntervalDays { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
