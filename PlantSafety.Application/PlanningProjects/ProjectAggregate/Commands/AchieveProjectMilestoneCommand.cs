// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           AchieveProjectMilestoneCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to mark a Project milestone as achieved. Same-status action, valid
//                  while Active. Implements IConcurrencyAwareCommand from the start.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Marks a Project milestone as achieved (valid while Active).</summary>
public sealed record AchieveProjectMilestoneCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ProjectId,
    Guid MilestoneId,
    Guid AchievedByUserId) : ICommand<Result<ProjectStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => AchievedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ProjectStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
