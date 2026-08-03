// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Infrastructure
//  File:           WorkflowInstanceDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IWorkflowInstanceRepository. All DB access via
//                  stored procedures in the [Workflow] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator, mirroring RiskDapperRepository's identical
//                  precedent. Context data is persisted as a JSON blob column and
//                  serialized/deserialized via System.Text.Json (global-used by this project).
//                  ListAsync evaluates the supplied WorkflowInstanceSpecification in-memory over
//                  the full non-deleted candidate set, matching IRiskRepository.ListAsync.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Entities;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Events;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.Specifications;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Workflow.Persistence;

/// <summary>Dapper-backed repository for the <see cref="WorkflowInstance"/> aggregate root.</summary>
public sealed class WorkflowInstanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WorkflowInstanceDapperRepository> logger)
    : IWorkflowInstanceRepository
{
    /// <inheritdoc/>
    public async Task<WorkflowInstance?> GetByIdAsync(WorkflowInstanceId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WorkflowInstance {WorkflowInstanceId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowInstanceId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkflowInstanceRow>(
            "[Workflow].[usp_WorkflowInstance_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var stepRows = await GetStepInstanceRowsAsync(id, cancellationToken);
        return Hydrate(row, stepRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkflowInstance>> ListAsync(Guid tenantId, WorkflowInstanceSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List WorkflowInstance via specification {Specification} for Tenant {TenantId}", specification.GetType().Name, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<WorkflowInstanceRow>(
            "[Workflow].[usp_WorkflowInstance_List]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<WorkflowInstance>();

        foreach (var row in rows)
        {
            var stepRows = await GetStepInstanceRowsAsync(WorkflowInstanceId.FromGuid(row.WorkflowInstanceId), cancellationToken);
            var aggregate = Hydrate(row, stepRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(WorkflowInstance aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add WorkflowInstance {WorkflowInstanceId}", aggregate.WorkflowInstanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkflowInstanceId", aggregate.WorkflowInstanceId.Value)
            .AddGuid("WorkflowDefinitionId", aggregate.WorkflowDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("WorkflowType", aggregate.WorkflowType.Code, 30)
            .AddString("State", aggregate.State.Code, 30)
            .AddGuid("CurrentStepId", aggregate.CurrentStepId?.Value)
            .AddString("ContextDataJson", JsonSerializer.Serialize(aggregate.ContextData), size: -1)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("InitiatedBy", aggregate.InitiatedBy.Value)
            .AddGuid("CorrelationId", aggregate.CorrelationId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Workflow].[usp_WorkflowInstance_Insert]", parameters, tx, cancellationToken: cancellationToken);

        var seq = 0;
        foreach (var step in aggregate.StepHistory)
            await UpsertStepInstanceAsync(conn, tx, step, seq++, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(WorkflowInstance aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update WorkflowInstance {WorkflowInstanceId}", aggregate.WorkflowInstanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkflowInstanceId", aggregate.WorkflowInstanceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("State", aggregate.State.Code, 30)
            .AddGuid("CurrentStepId", aggregate.CurrentStepId?.Value)
            .AddString("ContextDataJson", JsonSerializer.Serialize(aggregate.ContextData), size: -1)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync -- SET NOCOUNT ON makes ExecuteNonQuery always
        // return -1, which would silently disable the @ETag concurrency check.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Workflow].[usp_WorkflowInstance_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"WorkflowInstance {aggregate.WorkflowInstanceId.Value} was modified by another process or the ETag is stale.", Guid.Empty);
        }

        var seq = 0;
        foreach (var step in aggregate.StepHistory)
            await UpsertStepInstanceAsync(conn, tx, step, seq++, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    private async Task UpsertStepInstanceAsync(IDbConnection conn, IDbTransaction tx, WorkflowStepInstance step, int sequenceNo, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("StepInstanceId", step.StepInstanceId.Value)
            .AddGuid("WorkflowInstanceId", step.WorkflowInstanceId.Value)
            .AddGuid("StepId", step.StepId.Value)
            .AddString("StepType", step.StepType.Code, 30)
            .AddString("Status", step.Status.ToString(), 30)
            .AddString("AssignedRoleCode", step.AssignedActor.RoleCode, 100)
            .AddGuid("AssignedUserId", step.AssignedActor.UserId?.Value)
            .AddDateTime("ActivatedAtUtc", step.ActivatedAtUtc?.Value)
            .AddDateTime("DecidedAtUtc", step.DecidedAtUtc?.Value)
            .AddGuid("DecidedBy", step.DecidedBy?.Value)
            .AddString("DecisionNotes", step.DecisionNotes, 2000)
            .AddInt("AttemptCount", step.AttemptCount)
            .AddDateTime("NextAttemptAtUtc", step.NextAttemptAtUtc?.Value)
            .AddString("RecommendationReference", step.RecommendationReference, 400)
            .AddInt("SequenceNo", sequenceNo)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Workflow].[usp_WorkflowStepInstance_Upsert]", parameters, tx, cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowStepInstanceRow>> GetStepInstanceRowsAsync(WorkflowInstanceId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowInstanceId", id.Value).Build();
        return await queryExecutor.QueryAsync<WorkflowStepInstanceRow>(
            "[Workflow].[usp_WorkflowStepInstance_GetByInstanceId]", parameters, cancellationToken);
    }

    private static WorkflowInstance Hydrate(WorkflowInstanceRow row, IReadOnlyList<WorkflowStepInstanceRow> stepRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<WorkflowInstance>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.WorkflowInstanceId), WorkflowInstanceId.FromGuid(row.WorkflowInstanceId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.WorkflowDefinitionId), WorkflowDefinitionId.FromGuid(row.WorkflowDefinitionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.WorkflowType), WorkflowType.From(row.WorkflowType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.State), Domain.Workflow.Enums.WorkflowState.From(row.State));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.CurrentStepId), row.CurrentStepId.HasValue ? WorkflowStepId.FromGuid(row.CurrentStepId.Value) : (WorkflowStepId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.StartedAtUtc), UtcTimestamp.From(row.StartedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.InitiatedBy), new UserId(row.InitiatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowInstance.CorrelationId), row.CorrelationId);

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IWorkflowDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_stepHistory", new List<WorkflowStepInstance>());

        var contextData = string.IsNullOrWhiteSpace(row.ContextDataJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(row.ContextDataJson) ?? new Dictionary<string, string>();
        AggregateReflectionHydrator.SetField(agg, "_contextData", contextData);

        var history = AggregateReflectionHydrator.GetPrivateCollectionField<WorkflowInstance, List<WorkflowStepInstance>>(agg, "_stepHistory");
        foreach (var s in stepRows.OrderBy(x => x.SequenceNo))
            history.Add(HydrateStepInstance(s));

        return agg;
    }

    private static WorkflowStepInstance HydrateStepInstance(WorkflowStepInstanceRow row)
    {
        var step = AggregateReflectionHydrator.CreateUninitialized<WorkflowStepInstance>();

        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.StepInstanceId), WorkflowStepInstanceId.FromGuid(row.StepInstanceId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.WorkflowInstanceId), WorkflowInstanceId.FromGuid(row.WorkflowInstanceId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.StepId), WorkflowStepId.FromGuid(row.StepId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.StepType), WorkflowStepType.From(row.StepType));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.Status), Enum.Parse<WorkflowStepStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.AssignedActor),
            row.AssignedUserId is { } uid ? AssignedActor.ForUser(new UserId(uid)) : row.AssignedRoleCode is not null ? AssignedActor.ForRole(row.AssignedRoleCode) : AssignedActor.Unassigned);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.ActivatedAtUtc), row.ActivatedAtUtc.HasValue ? UtcTimestamp.From(row.ActivatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.DecidedAtUtc), row.DecidedAtUtc.HasValue ? UtcTimestamp.From(row.DecidedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.DecidedBy), row.DecidedBy.HasValue ? new UserId(row.DecidedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.DecisionNotes), row.DecisionNotes);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.AttemptCount), row.AttemptCount);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.NextAttemptAtUtc), row.NextAttemptAtUtc.HasValue ? UtcTimestamp.From(row.NextAttemptAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepInstance.RecommendationReference), row.RecommendationReference);

        return step;
    }

    private sealed record WorkflowInstanceRow
    {
        public required Guid WorkflowInstanceId { get; init; }
        public required Guid WorkflowDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string WorkflowType { get; init; }
        public required string State { get; init; }
        public Guid? CurrentStepId { get; init; }
        public required string ContextDataJson { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required Guid InitiatedBy { get; init; }
        public required Guid CorrelationId { get; init; }
        public string? ETag { get; init; }
    }

    private sealed record WorkflowStepInstanceRow
    {
        public required Guid StepInstanceId { get; init; }
        public required Guid WorkflowInstanceId { get; init; }
        public required Guid StepId { get; init; }
        public required string StepType { get; init; }
        public required string Status { get; init; }
        public string? AssignedRoleCode { get; init; }
        public Guid? AssignedUserId { get; init; }
        public DateTime? ActivatedAtUtc { get; init; }
        public DateTime? DecidedAtUtc { get; init; }
        public Guid? DecidedBy { get; init; }
        public string? DecisionNotes { get; init; }
        public required int AttemptCount { get; init; }
        public DateTime? NextAttemptAtUtc { get; init; }
        public string? RecommendationReference { get; init; }
        public required int SequenceNo { get; init; }
    }
}
