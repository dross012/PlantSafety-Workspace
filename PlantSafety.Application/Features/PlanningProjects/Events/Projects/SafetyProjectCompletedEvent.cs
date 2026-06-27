// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a SafetyProject transitions to Completed.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.PlanningProjects.Events.Projects;

/// <summary>Published via outbox when a safety project is completed.</summary>
public sealed record SafetyProjectCompletedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "PlanningProjects.SafetyProjectCompleted.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <summary>Completed project identifier.</summary>
    public required Guid ProjectId { get; init; }

    /// <summary>Project title.</summary>
    public required string Title { get; init; }

    /// <summary>Actor who completed the project.</summary>
    public required Guid CompletedByActorId { get; init; }

    /// <summary>UTC completion timestamp.</summary>
    public required DateTimeOffset CompletedAtUtc { get; init; }
}
