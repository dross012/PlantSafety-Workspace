// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Infrastructure
//  File:           ChecklistCompletionDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ChecklistCompletion aggregate root. All DB
//                  access via stored procedures in the [Checklists] schema. Aggregate
//                  rehydration uses AggregateReflectionHydrator because ChecklistCompletion
//                  exposes no public Rehydrate()/factory path. Additive-only evolution.
//
//                  KNOWN LIMITATION (flagged, not silently absorbed): ListAsync() receives
//                  only a ChecklistCompletionSpecification (an in-memory Expression<Func<
//                  Aggregate,bool>> predicate), with no explicit tenantId parameter on the
//                  Domain repository interface. Dapper has no LINQ/Expression-to-SQL
//                  translator, so tenant scoping cannot be pushed into the specification's
//                  compiled predicate at the SQL layer. This implementation therefore calls
//                  the candidate-row stored procedure with TenantId = Guid.Empty, which
//                  structurally matches zero persisted rows (fails CLOSED -- returns an
//                  empty list rather than leaking cross-tenant rows). A correct, fully
//                  tenant-scoped ListAsync would require extending IChecklistCompletionRepository
//                  with an explicit tenantId parameter, which is out of scope here (Domain
//                  interface signatures must not be changed).
// ============================================================================
using PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Entities;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Enums;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Specifications;
using PlantSafety.Domain.Checklists.ChecklistCompletion.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Checklists.ChecklistCompletion.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ChecklistCompletionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ChecklistCompletionDapperRepository> logger)
    : IChecklistCompletionRepository
{
    /// <inheritdoc/>
    public async Task<Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion?> GetByIdAsync(
        ChecklistCompletionId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById ChecklistCompletion {ChecklistCompletionId}", id.Value);

        var headerParams = new StoredProcedureBuilder()
            .AddGuid("ChecklistCompletionId", id.Value)
            .Build();

        var header = await queryExecutor.QuerySingleOrDefaultAsync<ChecklistCompletionRow>(
            "[Checklists].[usp_ChecklistCompletion_GetById]", headerParams, cancellationToken);

        if (header is null)
            return null;

        var resultParams = new StoredProcedureBuilder()
            .AddGuid("ChecklistCompletionId", id.Value)
            .Build();

        var resultRows = await queryExecutor.QueryAsync<ChecklistCompletionItemResultRow>(
            "[Checklists].[usp_ChecklistCompletionItemResult_ListByCompletion]", resultParams, cancellationToken);

        return Hydrate(header, resultRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion>> ListAsync(
        ChecklistCompletionSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List ChecklistCompletion via specification {Specification}", specification.GetType().Name);

        var headerParams = new StoredProcedureBuilder()
            .AddGuid("TenantId", Guid.Empty)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var headers = await queryExecutor.QueryAsync<ChecklistCompletionRow>(
            "[Checklists].[usp_ChecklistCompletion_ListByTenant]", headerParams, cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion>();

        foreach (var header in headers)
        {
            var resultParams = new StoredProcedureBuilder()
                .AddGuid("ChecklistCompletionId", header.ChecklistCompletionId)
                .Build();

            var resultRows = await queryExecutor.QueryAsync<ChecklistCompletionItemResultRow>(
                "[Checklists].[usp_ChecklistCompletionItemResult_ListByCompletion]", resultParams, cancellationToken);

            var aggregate = Hydrate(header, resultRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create ChecklistCompletion {ChecklistCompletionId} for tenant {TenantId}", aggregate.ChecklistCompletionId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParams = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("ChecklistCompletionId", aggregate.ChecklistCompletionId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddGuid("ChecklistTemplateId", aggregate.ChecklistTemplateId)
            .AddInt("ItemsTotal", aggregate.ItemsTotal)
            .AddInt("ItemsPassed", aggregate.ItemsPassed)
            .AddInt("ItemsFailed", aggregate.ItemsFailed)
            .AddString("Outcome", aggregate.Outcome, 30)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistCompletion_Create]", headerParams, tx, cancellationToken: cancellationToken);

        await UpsertItemResultsAsync(conn, tx, aggregate, cancellationToken);

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
        Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ChecklistCompletion {ChecklistCompletionId}", aggregate.ChecklistCompletionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var headerParams = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("ChecklistCompletionId", aggregate.ChecklistCompletionId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddInt("ItemsTotal", aggregate.ItemsTotal)
            .AddInt("ItemsPassed", aggregate.ItemsPassed)
            .AddInt("ItemsFailed", aggregate.ItemsFailed)
            .AddString("Outcome", aggregate.Outcome, 30)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddInt("CriticalFailureCount", aggregate.CriticalFailureCount)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistCompletion_Update]", headerParams, tx, cancellationToken: cancellationToken);

        await UpsertItemResultsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();
    }

    private async Task UpsertItemResultsAsync(
        IDbConnection conn, IDbTransaction tx,
        Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate,
        CancellationToken cancellationToken)
    {
        foreach (var result in aggregate.ItemResults)
        {
            var resultParams = new StoredProcedureBuilder()
                .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
                .AddGuid("ItemResultId", result.ItemResultId)
                .AddGuid("ChecklistCompletionId", result.ChecklistCompletionId.Value)
                .AddGuid("ChecklistTemplateItemId", result.ChecklistTemplateItemId)
                .AddBool("Passed", result.Passed)
                .AddBool("IsCriticalFailure", result.IsCriticalFailure)
                .AddString("Notes", result.Notes, 2000)
                .AddDateTime("RecordedAtUtc", result.RecordedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Checklists].[usp_ChecklistCompletionItemResult_Upsert]", resultParams, tx, cancellationToken: cancellationToken);
        }
    }

    private static Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion Hydrate(
        ChecklistCompletionRow header, IReadOnlyList<ChecklistCompletionItemResultRow> resultRows)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.ChecklistCompletionId), ChecklistCompletionId.FromGuid(header.ChecklistCompletionId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.TenantId), header.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.SiteId), header.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.Status), Enum.Parse<ChecklistCompletionStatus>(header.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.ChecklistTemplateId), header.ChecklistTemplateId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.ItemsTotal), header.ItemsTotal);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.ItemsPassed), header.ItemsPassed);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.ItemsFailed), header.ItemsFailed);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.Outcome), header.Outcome);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.CreatedAtUtc), new UtcTimestamp(header.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.CreatedBy), new UserId(header.CreatedBy));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.StartedAtUtc), header.StartedAtUtc.HasValue ? new UtcTimestamp(header.StartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.CompletedAtUtc), header.CompletedAtUtc.HasValue ? new UtcTimestamp(header.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.CriticalFailureCount), header.CriticalFailureCount);

        // Rehydration bug fix: GetPrivateCollectionField only reads the current field value (it
        // does not create one), and RuntimeHelpers.GetUninitializedObject bypasses field
        // initializers entirely, so "_itemResults" must be seeded first or this throws
        // InvalidOperationException ("Field '_itemResults' ... was null.").
        AggregateReflectionHydrator.SetField(aggregate, "_itemResults", new List<ChecklistCompletionItemResult>());
        var results = AggregateReflectionHydrator.GetPrivateCollectionField<Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, List<ChecklistCompletionItemResult>>(aggregate, "_itemResults");
        foreach (var row in resultRows)
        {
            results.Add(new ChecklistCompletionItemResult(
                itemResultId: row.ItemResultId,
                checklistCompletionId: ChecklistCompletionId.FromGuid(row.ChecklistCompletionId),
                checklistTemplateItemId: row.ChecklistTemplateItemId,
                passed: row.Passed,
                isCriticalFailure: row.IsCriticalFailure,
                notes: row.Notes,
                recordedAtUtc: new UtcTimestamp(row.RecordedAtUtc)));
        }

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Auditing.AuditEngagement aggregate): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // would throw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.Checklists.ChecklistCompletion.Events.IChecklistCompletionDomainEvent>());

        return aggregate;
    }

    private sealed record ChecklistCompletionRow
    {
        public required Guid ChecklistCompletionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid ChecklistTemplateId { get; init; }
        public required int ItemsTotal { get; init; }
        public required int ItemsPassed { get; init; }
        public required int ItemsFailed { get; init; }
        public required string Outcome { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required int CriticalFailureCount { get; init; }
    }

    private sealed record ChecklistCompletionItemResultRow
    {
        public required Guid ItemResultId { get; init; }
        public required Guid ChecklistCompletionId { get; init; }
        public required Guid ChecklistTemplateItemId { get; init; }
        public required bool Passed { get; init; }
        public required bool IsCriticalFailure { get; init; }
        public string? Notes { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
    }
}
