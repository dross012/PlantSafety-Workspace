// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Infrastructure
//  File:           PPETrainingFitDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPETrainingFitRepository (Domain repository
//                  interface for the PPETrainingFit aggregate root). All DB access via stored
//                  procedures in the [PPE] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPETrainingFit exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PPE.PPETrainingFit.Aggregates;
using PlantSafety.Domain.PPE.PPETrainingFit.Enums;
using PlantSafety.Domain.PPE.PPETrainingFit.Events;
using PlantSafety.Domain.PPE.PPETrainingFit.Interfaces;
using PlantSafety.Domain.PPE.PPETrainingFit.Specifications;
using PlantSafety.Domain.PPE.PPETrainingFit.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PPE.PPETrainingFit.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPETrainingFit"/> aggregate root.</summary>
public sealed class PPETrainingFitDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPETrainingFitDapperRepository> logger)
    : IPPETrainingFitRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit?> GetByIdAsync(PPETrainingFitId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPETrainingFit {PPETrainingFitId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPETrainingFitId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPETrainingFitRow>(
            "[PPE].[usp_PPETrainingFit_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit>> ListAsync(PPETrainingFitSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPETrainingFit via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPETrainingFitRow>(
            "[PPE].[usp_PPETrainingFit_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPETrainingFit {PPETrainingFitId}", aggregate.PPETrainingFitId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPETrainingFitId", aggregate.PPETrainingFitId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId)
            .AddString("TrainingType", aggregate.TrainingType, 400)
            .AddDateTime("FitTestExpiresAtUtc", aggregate.FitTestExpiresAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPETrainingFit_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPETrainingFit {PPETrainingFitId}", aggregate.PPETrainingFitId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPETrainingFitId", aggregate.PPETrainingFitId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId)
            .AddString("TrainingType", aggregate.TrainingType, 400)
            .AddDateTime("FitTestExpiresAtUtc", aggregate.FitTestExpiresAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPETrainingFit_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PPETrainingFit"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit Hydrate(PPETrainingFitRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit>();

        AggregateReflectionHydrator.SetProperty(agg, "PPETrainingFitId", PPETrainingFitId.FromGuid(row.PPETrainingFitId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPETrainingFitStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "WorkerId", row.WorkerId);
        AggregateReflectionHydrator.SetProperty(agg, "TrainingType", row.TrainingType);
        AggregateReflectionHydrator.SetProperty(agg, "FitTestExpiresAtUtc", row.FitTestExpiresAtUtc.HasValue ? UtcTimestamp.From(row.FitTestExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPETrainingFitDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPETrainingFit_GetById</c>/<c>usp_PPETrainingFit_GetAllActive</c>.</summary>
    private sealed record PPETrainingFitRow
    {
        public required Guid PPETrainingFitId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid WorkerId { get; init; }
        public required string TrainingType { get; init; }
        public DateTime? FitTestExpiresAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
