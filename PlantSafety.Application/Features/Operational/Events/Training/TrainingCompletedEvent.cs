// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a training record is marked completed.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Events;

namespace PlantSafety.Application.Features.Operational.Events.Training;

/// <summary>Published via outbox when a worker completes a training course.</summary>
public sealed record TrainingCompletedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc/>
    public string EventType => "Operational.TrainingCompleted.v1";

    /// <inheritdoc/>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <summary>Training record identifier.</summary>
    public required Guid RecordId { get; init; }

    /// <summary>Worker who completed the training.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Training course code.</summary>
    public required string TrainingCourseCode { get; init; }

    /// <summary>Training completion score (0-100).</summary>
    public required int? Score { get; init; }

    /// <summary>UTC completion timestamp.</summary>
    public required DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>UTC expiry timestamp for recurrence-based training.</summary>
    public required DateTimeOffset? ExpiresAtUtc { get; init; }
}
