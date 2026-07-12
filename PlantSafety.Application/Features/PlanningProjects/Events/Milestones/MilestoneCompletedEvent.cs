// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a project milestone is completed.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.PlanningProjects.Events.Milestones;

/// <summary>Published via outbox when a safety project milestone is completed.</summary>
public sealed record MilestoneCompletedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "PlanningProjects.MilestoneCompleted.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "Milestone";

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>Parent project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Completed milestone identifier.</summary>
    public required Guid MilestoneId { get; init; }

    /// <summary>Milestone title.</summary>
    public required string Title { get; init; }

    /// <summary>Actor who completed the milestone.</summary>
    public required Guid CompletedByActorId { get; init; }
}
