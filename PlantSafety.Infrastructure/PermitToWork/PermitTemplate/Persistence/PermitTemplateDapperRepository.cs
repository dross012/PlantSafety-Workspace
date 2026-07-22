// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Infrastructure
//  File:           PermitTemplateDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPermitTemplateRepository (Domain repository
//                  interface for the PermitTemplate aggregate root). All DB access via stored
//                  procedures in the [PermitToWork] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PermitTemplate exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates;
using PlantSafety.Domain.PermitToWork.PermitTemplate.Enums;
using PlantSafety.Domain.PermitToWork.PermitTemplate.Events;
using PlantSafety.Domain.PermitToWork.PermitTemplate.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitTemplate.Specifications;
using PlantSafety.Domain.PermitToWork.PermitTemplate.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PermitToWork.PermitTemplate.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PermitTemplate"/> aggregate root.</summary>
public sealed class PermitTemplateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitTemplateDapperRepository> logger)
    : IPermitTemplateRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate?> GetByIdAsync(PermitTemplateId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PermitTemplate {PermitTemplateId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PermitTemplateId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitTemplateRow>(
            "[PermitToWork].[usp_PermitTemplate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate>> ListAsync(PermitTemplateSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PermitTemplate via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PermitTemplateRow>(
            "[PermitToWork].[usp_PermitTemplate_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PermitTemplate {PermitTemplateId}", aggregate.PermitTemplateId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitTemplateId", aggregate.PermitTemplateId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitTypeCode", aggregate.PermitTypeCode, 400)
            .AddString("RequiredControls", aggregate.RequiredControls, 400)
            .AddInt("StandardValidityHours", aggregate.StandardValidityHours)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitTemplate_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PermitTemplate {PermitTemplateId}", aggregate.PermitTemplateId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitTemplateId", aggregate.PermitTemplateId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitTypeCode", aggregate.PermitTypeCode, 400)
            .AddString("RequiredControls", aggregate.RequiredControls, 400)
            .AddInt("StandardValidityHours", aggregate.StandardValidityHours)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitTemplate_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="PermitTemplate"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate Hydrate(PermitTemplateRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate>();

        AggregateReflectionHydrator.SetProperty(agg, "PermitTemplateId", PermitTemplateId.FromGuid(row.PermitTemplateId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PermitTemplateStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PermitTypeCode", row.PermitTypeCode);
        AggregateReflectionHydrator.SetProperty(agg, "RequiredControls", row.RequiredControls);
        AggregateReflectionHydrator.SetProperty(agg, "StandardValidityHours", row.StandardValidityHours);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPermitTemplateDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PermitTemplate_GetById</c>/<c>usp_PermitTemplate_GetAllActive</c>.</summary>
    private sealed record PermitTemplateRow
    {
        public required Guid PermitTemplateId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PermitTypeCode { get; init; }
        public required string RequiredControls { get; init; }
        public required int StandardValidityHours { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
