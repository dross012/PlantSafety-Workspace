// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Infrastructure
//  File:           OptimizationRunDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IOptimizationRunRepository (Domain repository
//                  interface for the OptimizationRun aggregate root). All DB access via stored
//                  procedures in the [Scheduling] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because OptimizationRun exposes only a private
//                  'for rehydration only' constructor and private-setter properties/backing
//                  fields, mirroring WorkflowInstanceDapperRepository's identical precedent.
//                  Input/result value-object collections are persisted as JSON blob columns and
//                  serialized/deserialized via System.Text.Json (global-used by this project).
//                  ListAsync evaluates the supplied OptimizationRunSpecification in-memory over
//                  the tenant's candidate set returned by usp_OptimizationRun_List.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.DomainEvents;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Specifications;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.PredictiveScheduling.Persistence;

/// <summary>Dapper-backed repository for the <see cref="OptimizationRun"/> aggregate root.</summary>
public sealed class OptimizationRunDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<OptimizationRunDapperRepository> logger)
    : IOptimizationRunRepository
{
    /// <inheritdoc/>
    public async Task<OptimizationRun?> GetByIdAsync(OptimizationRunId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for OptimizationRun {OptimizationRunId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("OptimizationRunId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<OptimizationRunRow>(
            "[Scheduling].[usp_OptimizationRun_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OptimizationRun>> ListAsync(Guid tenantId, OptimizationRunSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List OptimizationRun via specification {Specification} for Tenant {TenantId}", specification.GetType().Name, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<OptimizationRunRow>(
            "[Scheduling].[usp_OptimizationRun_List]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(OptimizationRun aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add OptimizationRun {OptimizationRunId}", aggregate.OptimizationRunId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OptimizationRunId", aggregate.OptimizationRunId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Objective", aggregate.Objective.ToString(), 30)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("SelectedStrategyName", aggregate.SelectedStrategyName, 100)
            .AddDouble("Score", aggregate.Score)
            .AddString("FailureReason", aggregate.FailureReason, 1000)
            .AddInt("InputTaskCount", aggregate.InputTasks.Count)
            .AddInt("InputAvailabilityCount", aggregate.InputAvailability.Count)
            .AddInt("AssignmentCount", aggregate.ResultAssignments.Count)
            .AddInt("UnscheduledTaskCount", aggregate.UnscheduledTaskIds.Count)
            .AddInt("ToleratedViolationCount", aggregate.ToleratedViolations.Count)
            .AddString("InputTasksJson", JsonSerializer.Serialize(aggregate.InputTasks.Select(TaskJson.From)), size: -1)
            .AddString("InputAvailabilityJson", JsonSerializer.Serialize(aggregate.InputAvailability.Select(WindowJson.From)), size: -1)
            .AddString("ResultAssignmentsJson", JsonSerializer.Serialize(aggregate.ResultAssignments.Select(AssignmentJson.From)), size: -1)
            .AddString("UnscheduledTaskIdsJson", JsonSerializer.Serialize(aggregate.UnscheduledTaskIds.Select(id => id.Value)), size: -1)
            .AddString("ToleratedViolationsJson", JsonSerializer.Serialize(aggregate.ToleratedViolations.Select(ViolationJson.From)), size: -1)
            .AddDateTime("RequestedAtUtc", aggregate.RequestedAtUtc.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("RequestedBy", aggregate.RequestedBy?.Value)
            .AddGuid("CorrelationId", aggregate.CorrelationId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_OptimizationRun_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(OptimizationRun aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update OptimizationRun {OptimizationRunId}", aggregate.OptimizationRunId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OptimizationRunId", aggregate.OptimizationRunId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("SelectedStrategyName", aggregate.SelectedStrategyName, 100)
            .AddDouble("Score", aggregate.Score)
            .AddString("FailureReason", aggregate.FailureReason, 1000)
            .AddInt("AssignmentCount", aggregate.ResultAssignments.Count)
            .AddInt("UnscheduledTaskCount", aggregate.UnscheduledTaskIds.Count)
            .AddInt("ToleratedViolationCount", aggregate.ToleratedViolations.Count)
            .AddString("ResultAssignmentsJson", JsonSerializer.Serialize(aggregate.ResultAssignments.Select(AssignmentJson.From)), size: -1)
            .AddString("UnscheduledTaskIdsJson", JsonSerializer.Serialize(aggregate.UnscheduledTaskIds.Select(id => id.Value)), size: -1)
            .AddString("ToleratedViolationsJson", JsonSerializer.Serialize(aggregate.ToleratedViolations.Select(ViolationJson.From)), size: -1)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_OptimizationRun_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="OptimizationRun"/> aggregate from its flat read-model row.</summary>
    private static OptimizationRun Hydrate(OptimizationRunRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<OptimizationRun>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.OptimizationRunId), OptimizationRunId.FromGuid(row.OptimizationRunId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.Objective), Enum.Parse<OptimizationObjective>(row.Objective));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.Status), Enum.Parse<OptimizationRunStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.SelectedStrategyName), row.SelectedStrategyName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.Score), row.Score);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.FailureReason), row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.RequestedAtUtc), UtcTimestamp.From(row.RequestedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.RequestedBy), row.RequestedBy.HasValue ? new UserId(row.RequestedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OptimizationRun.CorrelationId), row.CorrelationId);

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IOptimizationRunDomainEvent>());
        SeedCollectionFields(agg);

        var inputTasks = AggregateReflectionHydrator.GetPrivateCollectionField<OptimizationRun, List<SchedulableTask>>(agg, "_inputTasks");
        inputTasks.AddRange((JsonSerializer.Deserialize<List<TaskJson>>(row.InputTasksJson) ?? []).Select(t => t.ToDomain()));

        var inputAvailability = AggregateReflectionHydrator.GetPrivateCollectionField<OptimizationRun, List<ResourceAvailabilityWindow>>(agg, "_inputAvailability");
        inputAvailability.AddRange((JsonSerializer.Deserialize<List<WindowJson>>(row.InputAvailabilityJson) ?? []).Select(w => w.ToDomain()));

        var resultAssignments = AggregateReflectionHydrator.GetPrivateCollectionField<OptimizationRun, List<ScheduleAssignment>>(agg, "_resultAssignments");
        resultAssignments.AddRange((JsonSerializer.Deserialize<List<AssignmentJson>>(row.ResultAssignmentsJson) ?? []).Select(a => a.ToDomain()));

        var unscheduledTaskIds = AggregateReflectionHydrator.GetPrivateCollectionField<OptimizationRun, List<SchedulableTaskId>>(agg, "_unscheduledTaskIds");
        unscheduledTaskIds.AddRange((JsonSerializer.Deserialize<List<Guid>>(row.UnscheduledTaskIdsJson) ?? []).Select(SchedulableTaskId.FromGuid));

        var toleratedViolations = AggregateReflectionHydrator.GetPrivateCollectionField<OptimizationRun, List<ConstraintViolation>>(agg, "_toleratedViolations");
        toleratedViolations.AddRange((JsonSerializer.Deserialize<List<ViolationJson>>(row.ToleratedViolationsJson) ?? []).Select(v => v.ToDomain()));

        return agg;
    }

    /// <summary>
    /// Seeds every private backing-collection field before any of them is fetched via
    /// <see cref="AggregateReflectionHydrator.GetPrivateCollectionField{T, TCollection}"/> --
    /// <see cref="AggregateReflectionHydrator.CreateUninitialized{T}"/> bypasses all field
    /// initializers, so the fields start out null.
    /// </summary>
    private static void SeedCollectionFields(OptimizationRun agg)
    {
        AggregateReflectionHydrator.SetField(agg, "_inputTasks", new List<SchedulableTask>());
        AggregateReflectionHydrator.SetField(agg, "_inputAvailability", new List<ResourceAvailabilityWindow>());
        AggregateReflectionHydrator.SetField(agg, "_resultAssignments", new List<ScheduleAssignment>());
        AggregateReflectionHydrator.SetField(agg, "_unscheduledTaskIds", new List<SchedulableTaskId>());
        AggregateReflectionHydrator.SetField(agg, "_toleratedViolations", new List<ConstraintViolation>());
    }

    // ==== JSON projection shapes (avoid coupling Domain value objects to System.Text.Json attributes) ====

    private sealed record TaskJson(
        Guid TaskId, string Description, double EstimatedDurationMinutes, DateTime EarliestStartUtc, DateTime LatestFinishUtc,
        double BasePriorityWeight, int RequiredCrewSize, Guid? RequiredMachineId, Guid? RequiredCrewId,
        bool IsSafetyCritical, bool IsOverdue, int DeferralCount)
    {
        public static TaskJson From(SchedulableTask t) => new(
            t.TaskId.Value, t.Description, t.EstimatedDuration.TotalMinutes, t.EarliestStartUtc.Value, t.LatestFinishUtc.Value,
            t.BasePriorityWeight, t.RequiredCrewSize, t.RequiredMachineId?.Value, t.RequiredCrewId?.Value,
            t.IsSafetyCritical, t.IsOverdue, t.DeferralCount);

        public SchedulableTask ToDomain() => new(
            SchedulableTaskId.FromGuid(TaskId), Description, TimeSpan.FromMinutes(EstimatedDurationMinutes),
            UtcTimestamp.From(EarliestStartUtc), UtcTimestamp.From(LatestFinishUtc), BasePriorityWeight)
        {
            RequiredCrewSize = RequiredCrewSize,
            RequiredMachineId = RequiredMachineId is { } m ? SchedulingResourceId.FromGuid(m) : null,
            RequiredCrewId = RequiredCrewId is { } c ? SchedulingResourceId.FromGuid(c) : null,
            IsSafetyCritical = IsSafetyCritical,
            IsOverdue = IsOverdue,
            DeferralCount = DeferralCount,
        };
    }

    private sealed record WindowJson(Guid ResourceId, DateTime StartUtc, DateTime EndUtc, int Capacity)
    {
        public static WindowJson From(ResourceAvailabilityWindow w) => new(w.ResourceId.Value, w.StartUtc.Value, w.EndUtc.Value, w.Capacity);

        public ResourceAvailabilityWindow ToDomain() => new(
            SchedulingResourceId.FromGuid(ResourceId), UtcTimestamp.From(StartUtc), UtcTimestamp.From(EndUtc))
        {
            Capacity = Capacity,
        };
    }

    private sealed record AssignmentJson(Guid TaskId, Guid ResourceId, DateTime StartUtc, DateTime EndUtc, double EffectivePriorityWeight)
    {
        public static AssignmentJson From(ScheduleAssignment a) => new(a.TaskId.Value, a.ResourceId.Value, a.StartUtc.Value, a.EndUtc.Value, a.EffectivePriorityWeight);

        public ScheduleAssignment ToDomain() => new(
            SchedulableTaskId.FromGuid(TaskId), SchedulingResourceId.FromGuid(ResourceId), UtcTimestamp.From(StartUtc), UtcTimestamp.From(EndUtc))
        {
            EffectivePriorityWeight = EffectivePriorityWeight,
        };
    }

    private sealed record ViolationJson(string RuleName, string Severity, string Message, Guid TaskId)
    {
        public static ViolationJson From(ConstraintViolation v) => new(v.RuleName, v.Severity.ToString(), v.Message, v.TaskId.Value);

        public ConstraintViolation ToDomain() => new()
        {
            RuleName = RuleName,
            Severity = Enum.Parse<ConstraintSeverity>(Severity),
            Message = Message,
            TaskId = SchedulableTaskId.FromGuid(TaskId),
        };
    }

    /// <summary>Flat read-model row shape returned by usp_OptimizationRun_GetById/usp_OptimizationRun_List.</summary>
    private sealed record OptimizationRunRow
    {
        public required Guid OptimizationRunId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Objective { get; init; }
        public required string Status { get; init; }
        public string? SelectedStrategyName { get; init; }
        public double? Score { get; init; }
        public string? FailureReason { get; init; }
        public int InputTaskCount { get; init; }
        public int InputAvailabilityCount { get; init; }
        public int AssignmentCount { get; init; }
        public int UnscheduledTaskCount { get; init; }
        public int ToleratedViolationCount { get; init; }
        public required string InputTasksJson { get; init; }
        public required string InputAvailabilityJson { get; init; }
        public required string ResultAssignmentsJson { get; init; }
        public required string UnscheduledTaskIdsJson { get; init; }
        public required string ToleratedViolationsJson { get; init; }
        public required DateTime RequestedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? RequestedBy { get; init; }
        public required Guid CorrelationId { get; init; }
        public string? ETag { get; init; }
    }
}
