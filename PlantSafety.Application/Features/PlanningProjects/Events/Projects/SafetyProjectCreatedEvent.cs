// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a Safety Project is created.
// EventType: PlanningProjects.SafetyProjectCreated.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.PlanningProjects.Events.Projects;

/// <summary>
/// Raised after a Safety Project is created.
/// Consumers: Financials (budget allocation trigger), Analytics (project pipeline metrics).
/// </summary>
public sealed record SafetyProjectCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "PlanningProjects.SafetyProjectCreated.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "SafetyProject";

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>Primary site for the project.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Project priority.</summary>
    public required string Priority { get; init; }

    /// <summary>Project owner.</summary>
    public required Guid ProjectOwnerId { get; init; }

    /// <summary>Planned start date.</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Planned end date.</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }
}
