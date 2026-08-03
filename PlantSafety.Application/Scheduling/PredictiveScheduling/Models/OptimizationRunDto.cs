// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           OptimizationRunDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Input and read-model DTOs for the OptimizationRun aggregate. Input DTOs are
//                  transport-safe (untrusted) shapes bound at the API boundary and mapped to the
//                  real Domain value objects (SchedulableTask/ResourceAvailabilityWindow) by
//                  RequestOptimizationRunCommandHandler; read-model DTOs project the persisted
//                  aggregate for presentation and API responses. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

/// <summary>Caller-supplied input for a single schedulable task competing for placement in an optimization run.</summary>
/// <param name="TaskId">Identity of the task (mirrors the source aggregate identifier, e.g. a PMScheduling/WorkOrderPriority record).</param>
/// <param name="Description">Free-text label describing the source work item.</param>
/// <param name="EstimatedDurationMinutes">Estimated duration required to complete the task, in minutes.</param>
/// <param name="EarliestStartUtc">Earliest UTC instant at which the task may begin.</param>
/// <param name="LatestFinishUtc">Latest UTC instant by which the task must finish.</param>
/// <param name="BasePriorityWeight">Base priority weight before dynamic adjustment (higher schedules first).</param>
/// <param name="RequiredCrewSize">Number of crew members required to execute the task concurrently.</param>
/// <param name="RequiredMachineId">Identifier of the machine the task must be performed on, if machine-bound.</param>
/// <param name="RequiredCrewId">Identifier of the crew/team the task must be performed by, if crew-bound.</param>
/// <param name="IsSafetyCritical">True when the task involves hazard isolation, lockout/tagout, or another safety-critical procedure.</param>
/// <param name="IsOverdue">True when the source work item is already flagged overdue.</param>
/// <param name="DeferralCount">Number of times this task has previously slipped/been rescheduled.</param>
public sealed record SchedulableTaskInputDto(
    Guid TaskId,
    string Description,
    int EstimatedDurationMinutes,
    DateTime EarliestStartUtc,
    DateTime LatestFinishUtc,
    double BasePriorityWeight,
    int RequiredCrewSize,
    Guid? RequiredMachineId,
    Guid? RequiredCrewId,
    bool IsSafetyCritical,
    bool IsOverdue,
    int DeferralCount);

/// <summary>Caller-supplied input for a single resource availability window offered to an optimization run.</summary>
/// <param name="ResourceId">Identity of the resource this window belongs to.</param>
/// <param name="StartUtc">UTC start of the availability window.</param>
/// <param name="EndUtc">UTC end of the availability window.</param>
/// <param name="Capacity">Maximum concurrent task count the resource can support within this window.</param>
public sealed record ResourceAvailabilityWindowInputDto(
    Guid ResourceId,
    DateTime StartUtc,
    DateTime EndUtc,
    int Capacity);

/// <summary>Read-model DTO for a single task-to-resource placement produced by an optimization run.</summary>
/// <param name="TaskId">Identity of the placed task.</param>
/// <param name="ResourceId">Identity of the resource the task is placed against.</param>
/// <param name="StartUtc">UTC instant the task is planned to start.</param>
/// <param name="EndUtc">UTC instant the task is planned to finish.</param>
/// <param name="EffectivePriorityWeight">Composite priority weight used to order this assignment relative to others.</param>
public sealed record ScheduleAssignmentDto(
    Guid TaskId,
    Guid ResourceId,
    DateTime StartUtc,
    DateTime EndUtc,
    double EffectivePriorityWeight);

/// <summary>Read-model DTO for a single constraint rule violation tolerated by an optimization run's result.</summary>
/// <param name="RuleName">Stable machine-readable name of the rule that raised the violation.</param>
/// <param name="Severity">Severity classification code (Info/Warning/Blocking).</param>
/// <param name="Message">Human-readable explanation of the violation.</param>
/// <param name="TaskId">Identity of the task the violation pertains to.</param>
public sealed record ConstraintViolationDto(
    string RuleName,
    string Severity,
    string Message,
    Guid TaskId);

/// <summary>Full read-model DTO for a single OptimizationRun.</summary>
public sealed record OptimizationRunDto(
    Guid OptimizationRunId,
    Guid TenantId,
    Guid? SiteId,
    string Objective,
    string Status,
    string? SelectedStrategyName,
    double? Score,
    int InputTaskCount,
    int InputAvailabilityCount,
    IReadOnlyList<ScheduleAssignmentDto> Assignments,
    IReadOnlyList<Guid> UnscheduledTaskIds,
    IReadOnlyList<ConstraintViolationDto> ToleratedViolations,
    string? FailureReason,
    DateTime RequestedAtUtc,
    DateTime? CompletedAtUtc,
    Guid? RequestedBy,
    Guid CorrelationId);

/// <summary>Lightweight read-model DTO for a single OptimizationRun, used by the history/list query.</summary>
public sealed record OptimizationRunSummaryDto(
    Guid OptimizationRunId,
    string Objective,
    string Status,
    string? SelectedStrategyName,
    double? Score,
    int AssignmentCount,
    int UnscheduledTaskCount,
    DateTime RequestedAtUtc,
    DateTime? CompletedAtUtc);
