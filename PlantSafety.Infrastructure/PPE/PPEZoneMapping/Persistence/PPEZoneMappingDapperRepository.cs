// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Infrastructure
//  File:           PPEZoneMappingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPEZoneMappingRepository (Domain repository
//                  interface for the PPEZoneMapping aggregate root). All DB access via stored
//                  procedures in the [PPE] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPEZoneMapping exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates;
using PlantSafety.Domain.PPE.PPEZoneMapping.Enums;
using PlantSafety.Domain.PPE.PPEZoneMapping.Events;
using PlantSafety.Domain.PPE.PPEZoneMapping.Interfaces;
using PlantSafety.Domain.PPE.PPEZoneMapping.Specifications;
using PlantSafety.Domain.PPE.PPEZoneMapping.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PPE.PPEZoneMapping.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPEZoneMapping"/> aggregate root.</summary>
public sealed class PPEZoneMappingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPEZoneMappingDapperRepository> logger)
    : IPPEZoneMappingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping?> GetByIdAsync(PPEZoneMappingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPEZoneMapping {PPEZoneMappingId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPEZoneMappingId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPEZoneMappingRow>(
            "[PPE].[usp_PPEZoneMapping_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping>> ListAsync(PPEZoneMappingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPEZoneMapping via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPEZoneMappingRow>(
            "[PPE].[usp_PPEZoneMapping_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPEZoneMapping {PPEZoneMappingId}", aggregate.PPEZoneMappingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEZoneMappingId", aggregate.PPEZoneMappingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("RequiredPpeCategories", aggregate.RequiredPpeCategories, 400)
            .AddBool("AccessControlIntegrated", aggregate.AccessControlIntegrated)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEZoneMapping_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPEZoneMapping {PPEZoneMappingId}", aggregate.PPEZoneMappingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEZoneMappingId", aggregate.PPEZoneMappingId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("RequiredPpeCategories", aggregate.RequiredPpeCategories, 400)
            .AddBool("AccessControlIntegrated", aggregate.AccessControlIntegrated)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEZoneMapping_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PPEZoneMapping"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping Hydrate(PPEZoneMappingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping>();

        AggregateReflectionHydrator.SetProperty(agg, "PPEZoneMappingId", PPEZoneMappingId.FromGuid(row.PPEZoneMappingId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPEZoneMappingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "ZoneCode", row.ZoneCode);
        AggregateReflectionHydrator.SetProperty(agg, "RequiredPpeCategories", row.RequiredPpeCategories);
        AggregateReflectionHydrator.SetProperty(agg, "AccessControlIntegrated", row.AccessControlIntegrated);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPEZoneMappingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPEZoneMapping_GetById</c>/<c>usp_PPEZoneMapping_GetAllActive</c>.</summary>
    private sealed record PPEZoneMappingRow
    {
        public required Guid PPEZoneMappingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ZoneCode { get; init; }
        public required string RequiredPpeCategories { get; init; }
        public required bool AccessControlIntegrated { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
