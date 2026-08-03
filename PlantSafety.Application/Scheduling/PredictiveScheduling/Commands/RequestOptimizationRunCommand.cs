// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           RequestOptimizationRunCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Command to run the predictive scheduling optimization pipeline
//                  (MultiObjectiveScheduleOptimizer over the configured IOptimizationStrategy set)
//                  against a caller-supplied task/availability snapshot, and persist the resulting
//                  OptimizationRun so the decision becomes queryable/auditable.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Commands;

/// <summary>Runs the predictive scheduling optimization pipeline and persists the resulting OptimizationRun.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="Tasks">Candidate tasks competing for placement in this run.</param>
/// <param name="Availability">Declared resource availability windows for this run.</param>
/// <param name="ObjectiveCode">Name of the <see cref="Domain.Scheduling.PredictiveScheduling.Enums.OptimizationObjective"/> value to optimize for.</param>
/// <param name="RequestedByUserId">
/// Identity of the user who requested the run, or <see langword="null"/> for a system/cross-module
/// -triggered run (e.g. the reprioritization orchestrator's automated reoptimization).
/// </param>
public sealed record RequestOptimizationRunCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    IReadOnlyList<SchedulableTaskInputDto> Tasks,
    IReadOnlyList<ResourceAvailabilityWindowInputDto> Availability,
    string ObjectiveCode,
    Guid? RequestedByUserId) : ICommand<Result<OptimizationRunDto>>
{
    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<OptimizationRunDto>>.CausationId => CausationId;

    /// <summary>
    /// Gets the identifier of the actor who initiated the command. System/cross-module-triggered
    /// runs (<see cref="RequestedByUserId"/> is <see langword="null"/>) report <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid ActorId => RequestedByUserId ?? Guid.Empty;

    /// <summary>Builds a <see cref="RequestOptimizationRunCommand"/> from raw, untrusted transport values.</summary>
    public static RequestOptimizationRunCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<SchedulableTaskInputDto> tasks,
        IReadOnlyList<ResourceAvailabilityWindowInputDto> availability,
        string objectiveCode,
        Guid? requestedByUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            Tasks: tasks,
            Availability: availability,
            ObjectiveCode: objectiveCode,
            RequestedByUserId: requestedByUserId);
}
