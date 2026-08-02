// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Infrastructure
//  File:           StopWorkEventDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the StopWorkEvent aggregate root. All DB access via
//                  stored procedures in the [SafetyCulture] schema. InitiatedByActor
//                  (PerformedByActor value type) is flattened to two columns
//                  (InitiatedByActorKind, InitiatedByUserId) and reconstructed via
//                  PerformedByActor.User()/System()/AiAgent()/External(). Aggregate
//                  rehydration uses AggregateReflectionHydrator because StopWorkEvent
//                  exposes no public Rehydrate()/factory path. Additive-only evolution.
//
//                  KNOWN LIMITATION: see AuditEngagementDapperRepository -- ListAsync's
//                  specification-only signature has no tenantId, so it fails closed (empty
//                  result) rather than risk a cross-tenant leak.
// ============================================================================
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Enums;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Interfaces;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Specifications;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyCulture.StopWorkEvent.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class StopWorkEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<StopWorkEventDapperRepository> logger)
    : IStopWorkEventRepository
{
    /// <inheritdoc/>
    public async Task<Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent?> GetByIdAsync(
        StopWorkEventId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById StopWorkEvent {StopWorkEventId}", id.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("StopWorkEventId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<StopWorkEventRow>(
            "[SafetyCulture].[usp_StopWorkEvent_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent>> ListAsync(
        StopWorkEventSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List StopWorkEvent via specification {Specification}", specification.GetType().Name);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", Guid.Empty)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var rows = await queryExecutor.QueryAsync<StopWorkEventRow>(
            "[SafetyCulture].[usp_StopWorkEvent_ListByTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create StopWorkEvent {StopWorkEventId} for tenant {TenantId}", aggregate.StopWorkEventId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("StopWorkEventId", aggregate.StopWorkEventId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("WorkArea", aggregate.WorkArea, 200)
            .AddString("HazardDescription", aggregate.HazardDescription, 2000)
            .AddString("InitiatedByActorKind", aggregate.InitiatedByActor.Kind.ToString(), 20)
            .AddGuid("InitiatedByUserId", aggregate.InitiatedByActor.UserId?.Value)
            .AddBool("WasCelebrated", aggregate.WasCelebrated)
            .AddGuid("RelatedPermitId", aggregate.RelatedPermitId)
            .AddGuid("RelatedChecklistCompletionId", aggregate.RelatedChecklistCompletionId)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyCulture].[usp_StopWorkEvent_Create]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(
        Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update StopWorkEvent {StopWorkEventId}", aggregate.StopWorkEventId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("StopWorkEventId", aggregate.StopWorkEventId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("ResolutionSummary", aggregate.ResolutionSummary, 2000)
            .AddBool("WasCelebrated", aggregate.WasCelebrated)
            .AddString("InvestigationFindings", aggregate.InvestigationFindings, 2000)
            .AddDateTime("InvestigatedAtUtc", aggregate.InvestigatedAtUtc?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyCulture].[usp_StopWorkEvent_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private static Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent Hydrate(StopWorkEventRow row)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent>();

        var actorKind = Enum.Parse<ActorKind>(row.InitiatedByActorKind);
        var initiatedByActor = actorKind switch
        {
            ActorKind.User => PerformedByActor.User(new UserId(row.InitiatedByUserId!.Value)),
            ActorKind.AiAgent => PerformedByActor.AiAgent(),
            ActorKind.External => PerformedByActor.External(),
            _ => PerformedByActor.System(),
        };

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.StopWorkEventId), StopWorkEventId.FromGuid(row.StopWorkEventId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.Status), Enum.Parse<StopWorkEventStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.WorkArea), row.WorkArea);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.HazardDescription), row.HazardDescription);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.InitiatedByActor), initiatedByActor);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.ResolutionSummary), row.ResolutionSummary);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.WasCelebrated), row.WasCelebrated);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.RelatedPermitId), row.RelatedPermitId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.RelatedChecklistCompletionId), row.RelatedChecklistCompletionId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.InvestigationFindings), row.InvestigationFindings);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.InvestigatedAtUtc), row.InvestigatedAtUtc.HasValue ? new UtcTimestamp(row.InvestigatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? new UtcTimestamp(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.CreatedAtUtc), new UtcTimestamp(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent.CreatedBy), new UserId(row.CreatedBy));

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Auditing.AuditEngagement aggregate): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // would throw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.SafetyCulture.StopWorkEvent.Events.IStopWorkEventDomainEvent>());

        return aggregate;
    }

    private sealed record StopWorkEventRow
    {
        public required Guid StopWorkEventId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string WorkArea { get; init; }
        public required string HazardDescription { get; init; }
        public required string InitiatedByActorKind { get; init; }
        public Guid? InitiatedByUserId { get; init; }
        public string? ResolutionSummary { get; init; }
        public required bool WasCelebrated { get; init; }
        public Guid? RelatedPermitId { get; init; }
        public Guid? RelatedChecklistCompletionId { get; init; }
        public string? InvestigationFindings { get; init; }
        public DateTime? InvestigatedAtUtc { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
