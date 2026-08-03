// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           RequestOptimizationRunCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for RequestOptimizationRunCommand. Maps the caller-supplied task/
//                  availability DTOs to the real Domain value objects, constructs an
//                  OptimizationRun aggregate via its domain factory, runs the real
//                  MultiObjectiveScheduleOptimizer over the configured IOptimizationStrategy set,
//                  records the outcome onto the aggregate (Complete on success, Fail on exception),
//                  and persists it -- so the optimization decision is queryable/auditable rather
//                  than computed and discarded. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Commands;

/// <summary>Handles <see cref="RequestOptimizationRunCommand"/>.</summary>
public sealed class RequestOptimizationRunCommandHandler : IRequestHandler<RequestOptimizationRunCommand, Result<OptimizationRunDto>>
{
    private readonly IOptimizationRunFactory _factory;
    private readonly IOptimizationRunRepository _repository;
    private readonly MultiObjectiveScheduleOptimizer _optimizer;
    private readonly ILogger<RequestOptimizationRunCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RequestOptimizationRunCommandHandler(
        IOptimizationRunFactory factory,
        IOptimizationRunRepository repository,
        MultiObjectiveScheduleOptimizer optimizer,
        ILogger<RequestOptimizationRunCommandHandler> logger)
    {
        _factory = factory;
        _repository = repository;
        _optimizer = optimizer;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OptimizationRunDto>> Handle(RequestOptimizationRunCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OptimizationObjective>(request.ObjectiveCode, ignoreCase: true, out var objective))
        {
            return Result<OptimizationRunDto>.Failure(new ApplicationError(
                "PredictiveScheduling.OptimizationRun.InvalidObjective",
                $"'{request.ObjectiveCode}' is not a recognized OptimizationObjective.",
                request.CorrelationId,
                ErrorSeverity.Validation));
        }

        var nowUtc = UtcTimestamp.Now();
        var correlationId = new CorrelationId(request.CorrelationId);
        var causationId = new CausationId(request.CausationId);
        var actor = request.RequestedByUserId is { } uid ? PerformedByActor.User(new UserId(uid)) : PerformedByActor.System();

        try
        {
            var tasks = request.Tasks.Select(ToTask).ToList();
            var availability = request.Availability.Select(ToWindow).ToList();

            var run = _factory.Request(
                request.TenantId,
                request.SiteId,
                tasks,
                availability,
                objective,
                request.RequestedByUserId is { } u ? new UserId(u) : null,
                nowUtc,
                correlationId);

            run.Begin(nowUtc, actor, correlationId, causationId);

            try
            {
                var context = new OptimizationContext
                {
                    Tasks = tasks,
                    Availability = availability,
                    Objective = objective,
                    AsOfUtc = nowUtc,
                };

                var outcome = _optimizer.OptimizeBest(context);

                run.Complete(
                    outcome.SelectedStrategyName,
                    outcome.SelectedResult.Assignments,
                    outcome.SelectedResult.UnscheduledTaskIds,
                    outcome.SelectedResult.ToleratedViolations,
                    outcome.SelectedResult.Score,
                    UtcTimestamp.Now(),
                    actor,
                    correlationId,
                    causationId);
            }
            catch (Exception computeEx)
            {
                _logger.LogError(computeEx, "Optimization pipeline failed for tenant {TenantId}", request.TenantId);
                run.Fail(computeEx.Message, UtcTimestamp.Now(), actor, correlationId, causationId);
            }

            await _repository.AddAsync(run, cancellationToken);

            return Result<OptimizationRunDto>.Success(ToDto(run));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request OptimizationRun for tenant {TenantId}", request.TenantId);

            return Result<OptimizationRunDto>.Failure(new ApplicationError(
                "PredictiveScheduling.OptimizationRun.RequestFailed",
                "Unable to run the predictive scheduling optimization pipeline at this time.",
                request.CorrelationId));
        }
    }

    private static SchedulableTask ToTask(SchedulableTaskInputDto dto) => new(
        taskId: SchedulableTaskId.FromGuid(dto.TaskId == Guid.Empty ? Guid.NewGuid() : dto.TaskId),
        description: dto.Description,
        estimatedDuration: TimeSpan.FromMinutes(dto.EstimatedDurationMinutes),
        earliestStartUtc: UtcTimestamp.From(dto.EarliestStartUtc),
        latestFinishUtc: UtcTimestamp.From(dto.LatestFinishUtc),
        basePriorityWeight: dto.BasePriorityWeight)
    {
        RequiredCrewSize = dto.RequiredCrewSize <= 0 ? 1 : dto.RequiredCrewSize,
        RequiredMachineId = dto.RequiredMachineId is { } m ? SchedulingResourceId.FromGuid(m) : null,
        RequiredCrewId = dto.RequiredCrewId is { } c ? SchedulingResourceId.FromGuid(c) : null,
        IsSafetyCritical = dto.IsSafetyCritical,
        IsOverdue = dto.IsOverdue,
        DeferralCount = dto.DeferralCount,
    };

    private static ResourceAvailabilityWindow ToWindow(ResourceAvailabilityWindowInputDto dto) => new(
        resourceId: SchedulingResourceId.FromGuid(dto.ResourceId),
        startUtc: UtcTimestamp.From(dto.StartUtc),
        endUtc: UtcTimestamp.From(dto.EndUtc))
    {
        Capacity = dto.Capacity <= 0 ? 1 : dto.Capacity,
    };

    /// <summary>Projects a fully-hydrated <see cref="Domain.Scheduling.PredictiveScheduling.Aggregates.OptimizationRun"/> into its read-model DTO.</summary>
    internal static OptimizationRunDto ToDto(Domain.Scheduling.PredictiveScheduling.Aggregates.OptimizationRun run) => new(
        OptimizationRunId: run.OptimizationRunId.Value,
        TenantId: run.TenantId,
        SiteId: run.SiteId,
        Objective: run.Objective.ToString(),
        Status: run.Status.ToString(),
        SelectedStrategyName: run.SelectedStrategyName,
        Score: run.Score,
        InputTaskCount: run.InputTasks.Count,
        InputAvailabilityCount: run.InputAvailability.Count,
        Assignments: run.ResultAssignments.Select(a => new ScheduleAssignmentDto(
            a.TaskId.Value, a.ResourceId.Value, a.StartUtc.Value, a.EndUtc.Value, a.EffectivePriorityWeight)).ToList(),
        UnscheduledTaskIds: run.UnscheduledTaskIds.Select(id => id.Value).ToList(),
        ToleratedViolations: run.ToleratedViolations.Select(v => new ConstraintViolationDto(
            v.RuleName, v.Severity.ToString(), v.Message, v.TaskId.Value)).ToList(),
        FailureReason: run.FailureReason,
        RequestedAtUtc: run.RequestedAtUtc.Value,
        CompletedAtUtc: run.CompletedAtUtc?.Value,
        RequestedBy: run.RequestedBy?.Value,
        CorrelationId: run.CorrelationId);
}
