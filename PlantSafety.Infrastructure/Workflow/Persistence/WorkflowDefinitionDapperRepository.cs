// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Infrastructure
//  File:           WorkflowDefinitionDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IWorkflowDefinitionRepository. All DB access via
//                  stored procedures in the [Workflow] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because WorkflowDefinition/WorkflowStepDefinition
//                  expose only private/internal constructors and private-setter properties,
//                  mirroring RiskDapperRepository's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using System.Data;
using Dapper;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Entities;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Events;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Workflow.Persistence;

/// <summary>Dapper-backed repository for the <see cref="WorkflowDefinition"/> aggregate root.</summary>
public sealed class WorkflowDefinitionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WorkflowDefinitionDapperRepository> logger)
    : IWorkflowDefinitionRepository
{
    private const string StepTableType = "[Workflow].[WorkflowStepDefinitionTableType]";
    private const string TransitionTableType = "[Workflow].[WorkflowTransitionTableType]";

    /// <inheritdoc/>
    public async Task<WorkflowDefinition?> GetByIdAsync(WorkflowDefinitionId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WorkflowDefinition {WorkflowDefinitionId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowDefinitionId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkflowDefinitionRow>(
            "[Workflow].[usp_WorkflowDefinition_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var stepRows = await GetStepRowsAsync(id, cancellationToken);
        var transitionRows = await GetTransitionRowsAsync(id, cancellationToken);
        return Hydrate(row, stepRows, transitionRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkflowDefinition>> ListActiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListActive WorkflowDefinition for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<WorkflowDefinitionRow>(
            "[Workflow].[usp_WorkflowDefinition_GetAllActive]", parameters, cancellationToken);

        var results = new List<WorkflowDefinition>();
        foreach (var row in rows)
        {
            var id = WorkflowDefinitionId.FromGuid(row.WorkflowDefinitionId);
            var stepRows = await GetStepRowsAsync(id, cancellationToken);
            var transitionRows = await GetTransitionRowsAsync(id, cancellationToken);
            results.Add(Hydrate(row, stepRows, transitionRows));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(WorkflowDefinition aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add WorkflowDefinition {WorkflowDefinitionId}", aggregate.WorkflowDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkflowDefinitionId", aggregate.WorkflowDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("WorkflowType", aggregate.WorkflowType.Code, 30)
            .AddString("Name", aggregate.Name, 200)
            .AddString("Description", aggregate.Description, 2000)
            .AddInt("Version", aggregate.Version)
            .AddBool("IsActive", aggregate.IsActive)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Workflow].[usp_WorkflowDefinition_Insert]", parameters, tx, cancellationToken: cancellationToken);

        await BulkUpsertStepsAndTransitionsAsync(conn, tx, aggregate.WorkflowDefinitionId, aggregate.Steps, aggregate.Transitions, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(WorkflowDefinition aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update WorkflowDefinition {WorkflowDefinitionId}", aggregate.WorkflowDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkflowDefinitionId", aggregate.WorkflowDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddInt("Version", aggregate.Version)
            .AddBool("IsActive", aggregate.IsActive)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check. See usp_WorkflowDefinition_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Workflow].[usp_WorkflowDefinition_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"WorkflowDefinition {aggregate.WorkflowDefinitionId.Value} was modified by another process or the ETag is stale.", Guid.Empty);
        }

        // Steps/transitions are immutable once added; bulk-reconcile so newly-added ones (via
        // AddStep/AddTransition, which call this same UpdateAsync) are persisted in one
        // set-based round trip each instead of one stored-proc call per step/transition (a real
        // N+1 write pattern for workflow definitions with many steps -- see
        // usp_WorkflowStepDefinition_BulkUpsert.sql / usp_WorkflowTransition_BulkUpsert.sql).
        await BulkUpsertStepsAndTransitionsAsync(conn, tx, aggregate.WorkflowDefinitionId, aggregate.Steps, aggregate.Transitions, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <summary>
    /// Bulk-reconciles every step and transition on the aggregate in exactly two set-based round
    /// trips (one TVP-backed proc call each), replacing what was previously one
    /// usp_WorkflowStepDefinition_Upsert/usp_WorkflowTransition_Upsert call per step/transition
    /// (Handbook §7 Performance &amp; Stress -- Phase 5 §7.3 "bulk operations"). Both target procs
    /// preserve the original per-row procs' "insert if not already present" idempotent semantics
    /// (steps/transitions are immutable once added), just set-based instead of row-by-row. Mirrors
    /// ProjectAggregateDapperRepository's Milestones TVP pattern -- this codebase's only prior
    /// bulk-write precedent.
    /// </summary>
    private async Task BulkUpsertStepsAndTransitionsAsync(
        IDbConnection conn,
        IDbTransaction tx,
        WorkflowDefinitionId definitionId,
        IReadOnlyCollection<WorkflowStepDefinition> steps,
        IReadOnlyCollection<WorkflowTransition> transitions,
        CancellationToken cancellationToken)
    {
        if (steps.Count > 0)
        {
            var stepParameters = new DynamicParameters();
            stepParameters.Add("WorkflowDefinitionId", definitionId.Value);
            stepParameters.Add("Steps", BuildStepTable(steps).AsTableValuedParameter(StepTableType));

            await commandExecutor.ExecuteAsync(conn, "[Workflow].[usp_WorkflowStepDefinition_BulkUpsert]", stepParameters, tx, cancellationToken: cancellationToken);
        }

        if (transitions.Count > 0)
        {
            var transitionParameters = new DynamicParameters();
            transitionParameters.Add("WorkflowDefinitionId", definitionId.Value);
            transitionParameters.Add("Transitions", BuildTransitionTable(transitions).AsTableValuedParameter(TransitionTableType));

            await commandExecutor.ExecuteAsync(conn, "[Workflow].[usp_WorkflowTransition_BulkUpsert]", transitionParameters, tx, cancellationToken: cancellationToken);
        }
    }

    private static DataTable BuildStepTable(IReadOnlyCollection<WorkflowStepDefinition> steps)
    {
        var table = new DataTable();
        table.Columns.Add("StepId", typeof(Guid));
        table.Columns.Add("Order", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("StepType", typeof(string));
        table.Columns.Add("AssignedRoleCode", typeof(string));
        table.Columns.Add("AssignedUserId", typeof(Guid));
        table.Columns.Add("EscalationRoleCode", typeof(string));
        table.Columns.Add("EscalationUserId", typeof(Guid));
        table.Columns.Add("EntryConditionField", typeof(string));
        table.Columns.Add("EntryConditionOperator", typeof(string));
        table.Columns.Add("EntryConditionValue", typeof(string));
        table.Columns.Add("TimeoutSeconds", typeof(int));
        table.Columns.Add("TimeoutAction", typeof(string));
        table.Columns.Add("RetryMaxAttempts", typeof(int));
        table.Columns.Add("RetryBackoffStrategy", typeof(string));
        table.Columns.Add("RetryBaseDelaySeconds", typeof(int));

        foreach (var step in steps)
        {
            table.Rows.Add(
                step.StepId.Value,
                step.Order,
                step.Name,
                step.StepType.Code,
                (object?)step.AssignedActor.RoleCode ?? DBNull.Value,
                (object?)step.AssignedActor.UserId?.Value ?? DBNull.Value,
                (object?)step.EscalationActor.RoleCode ?? DBNull.Value,
                (object?)step.EscalationActor.UserId?.Value ?? DBNull.Value,
                (object?)(step.EntryCondition.IsUnconditional ? null : step.EntryCondition.FieldName) ?? DBNull.Value,
                (object?)(step.EntryCondition.IsUnconditional ? null : step.EntryCondition.Operator.ToString()) ?? DBNull.Value,
                (object?)(step.EntryCondition.IsUnconditional ? null : step.EntryCondition.ExpectedValue) ?? DBNull.Value,
                (object?)(step.Timeout is { } t ? (int)t.Duration.TotalSeconds : (int?)null) ?? DBNull.Value,
                (object?)step.Timeout?.Action.ToString() ?? DBNull.Value,
                (object?)step.Retry?.MaxAttempts ?? DBNull.Value,
                (object?)step.Retry?.Strategy.Code ?? DBNull.Value,
                (object?)(step.Retry is { } r ? (int)r.BaseDelay.TotalSeconds : (int?)null) ?? DBNull.Value);
        }

        return table;
    }

    private static DataTable BuildTransitionTable(IReadOnlyCollection<WorkflowTransition> transitions)
    {
        var table = new DataTable();
        table.Columns.Add("FromStepId", typeof(Guid));
        table.Columns.Add("ToStepId", typeof(Guid));
        table.Columns.Add("GuardField", typeof(string));
        table.Columns.Add("GuardOperator", typeof(string));
        table.Columns.Add("GuardExpectedValue", typeof(string));
        table.Columns.Add("IsDefault", typeof(bool));

        foreach (var transition in transitions)
        {
            table.Rows.Add(
                transition.FromStepId.Value,
                transition.ToStepId.Value,
                (object?)(transition.Guard.IsUnconditional ? null : transition.Guard.FieldName) ?? DBNull.Value,
                (object?)(transition.Guard.IsUnconditional ? null : transition.Guard.Operator.ToString()) ?? DBNull.Value,
                (object?)(transition.Guard.IsUnconditional ? null : transition.Guard.ExpectedValue) ?? DBNull.Value,
                transition.IsDefault);
        }

        return table;
    }

    private async Task<IReadOnlyList<WorkflowStepDefinitionRow>> GetStepRowsAsync(WorkflowDefinitionId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowDefinitionId", id.Value).Build();
        return await queryExecutor.QueryAsync<WorkflowStepDefinitionRow>(
            "[Workflow].[usp_WorkflowStepDefinition_GetByDefinitionId]", parameters, cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowTransitionRow>> GetTransitionRowsAsync(WorkflowDefinitionId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowDefinitionId", id.Value).Build();
        return await queryExecutor.QueryAsync<WorkflowTransitionRow>(
            "[Workflow].[usp_WorkflowTransition_GetByDefinitionId]", parameters, cancellationToken);
    }

    private static WorkflowDefinition Hydrate(WorkflowDefinitionRow row, IReadOnlyList<WorkflowStepDefinitionRow> stepRows, IReadOnlyList<WorkflowTransitionRow> transitionRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<WorkflowDefinition>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.WorkflowDefinitionId), WorkflowDefinitionId.FromGuid(row.WorkflowDefinitionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.WorkflowType), WorkflowType.From(row.WorkflowType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.Version), row.Version);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.IsActive), row.IsActive);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkflowDefinition.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IWorkflowDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_steps", new List<WorkflowStepDefinition>());
        AggregateReflectionHydrator.SetField(agg, "_transitions", new List<WorkflowTransition>());

        var steps = AggregateReflectionHydrator.GetPrivateCollectionField<WorkflowDefinition, List<WorkflowStepDefinition>>(agg, "_steps");
        foreach (var s in stepRows)
            steps.Add(HydrateStep(s));

        var transitions = AggregateReflectionHydrator.GetPrivateCollectionField<WorkflowDefinition, List<WorkflowTransition>>(agg, "_transitions");
        foreach (var t in transitionRows)
            transitions.Add(new WorkflowTransition(
                WorkflowStepId.FromGuid(t.FromStepId), WorkflowStepId.FromGuid(t.ToStepId), HydrateGuard(t.GuardField, t.GuardOperator, t.GuardExpectedValue), t.IsDefault));

        return agg;
    }

    private static WorkflowStepDefinition HydrateStep(WorkflowStepDefinitionRow row)
    {
        var step = AggregateReflectionHydrator.CreateUninitialized<WorkflowStepDefinition>();

        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.StepId), WorkflowStepId.FromGuid(row.StepId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.WorkflowDefinitionId), WorkflowDefinitionId.FromGuid(row.WorkflowDefinitionId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.Order), row.Order);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.StepType), WorkflowStepType.From(row.StepType));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.AssignedActor), HydrateActor(row.AssignedRoleCode, row.AssignedUserId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.EscalationActor), HydrateActor(row.EscalationRoleCode, row.EscalationUserId));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.EntryCondition), HydrateGuard(row.EntryConditionField, row.EntryConditionOperator, row.EntryConditionValue));
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.Timeout),
            row.TimeoutSeconds.HasValue && row.TimeoutAction is not null
                ? new TimeoutPolicy(TimeSpan.FromSeconds(row.TimeoutSeconds.Value), Enum.Parse<TimeoutAction>(row.TimeoutAction, ignoreCase: true))
                : (TimeoutPolicy?)null);
        AggregateReflectionHydrator.SetProperty(step, nameof(WorkflowStepDefinition.Retry),
            row.RetryMaxAttempts.HasValue && row.RetryBackoffStrategy is not null
                ? new RetryPolicy(row.RetryMaxAttempts.Value, RetryBackoffStrategy.From(row.RetryBackoffStrategy), TimeSpan.FromSeconds(row.RetryBaseDelaySeconds ?? 0))
                : (RetryPolicy?)null);

        return step;
    }

    private static AssignedActor HydrateActor(string? roleCode, Guid? userId)
        => userId is { } uid ? AssignedActor.ForUser(new UserId(uid)) : roleCode is not null ? AssignedActor.ForRole(roleCode) : AssignedActor.Unassigned;

    private static GuardCondition HydrateGuard(string? field, string? op, string? value)
        => string.IsNullOrWhiteSpace(field)
            ? GuardCondition.None
            : GuardCondition.Create(field, Enum.Parse<GuardOperator>(op ?? "Equals", ignoreCase: true), value ?? string.Empty);

    private sealed record WorkflowDefinitionRow
    {
        public required Guid WorkflowDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string WorkflowType { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required int Version { get; init; }
        public required bool IsActive { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public string? ETag { get; init; }
    }

    private sealed record WorkflowStepDefinitionRow
    {
        public required Guid StepId { get; init; }
        public required Guid WorkflowDefinitionId { get; init; }
        public required int Order { get; init; }
        public required string Name { get; init; }
        public required string StepType { get; init; }
        public string? AssignedRoleCode { get; init; }
        public Guid? AssignedUserId { get; init; }
        public string? EscalationRoleCode { get; init; }
        public Guid? EscalationUserId { get; init; }
        public string? EntryConditionField { get; init; }
        public string? EntryConditionOperator { get; init; }
        public string? EntryConditionValue { get; init; }
        public int? TimeoutSeconds { get; init; }
        public string? TimeoutAction { get; init; }
        public int? RetryMaxAttempts { get; init; }
        public string? RetryBackoffStrategy { get; init; }
        public int? RetryBaseDelaySeconds { get; init; }
    }

    private sealed record WorkflowTransitionRow
    {
        public required Guid WorkflowTransitionId { get; init; }
        public required Guid WorkflowDefinitionId { get; init; }
        public required Guid FromStepId { get; init; }
        public required Guid ToStepId { get; init; }
        public string? GuardField { get; init; }
        public string? GuardOperator { get; init; }
        public string? GuardExpectedValue { get; init; }
        public required bool IsDefault { get; init; }
    }
}
