// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a project milestone is completed.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.PlanningProjects.Events.Milestones;

/// <summary>Published via outbox when a safety project milestone is completed.</summary>
public sealed record MilestoneCompletedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "PlanningProjects.MilestoneCompleted.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <summary>Parent project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Completed milestone identifier.</summary>
    public required Guid MilestoneId { get; init; }

    /// <summary>Milestone title.</summary>
    public required string Title { get; init; }

    /// <summary>Actor who completed the milestone.</summary>
    public required Guid CompletedByActorId { get; init; }
}
