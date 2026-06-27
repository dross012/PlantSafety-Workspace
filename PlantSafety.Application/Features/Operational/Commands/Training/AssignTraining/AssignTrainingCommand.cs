// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to assign a training course to a worker.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Operational.Commands.Training.AssignTraining;

/// <summary>
/// Assigns a safety training course to a worker.
/// Caller must hold <c>Operational.Training.Assign</c>.
/// </summary>
public sealed record AssignTrainingCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Worker being assigned the training.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Training course to assign.</summary>
    public required Guid CourseId { get; init; }

    /// <summary>Delivery method (InPerson, Online, OJT).</summary>
    public required string DeliveryMethod { get; init; }

    /// <summary>Optional due date for completion (UTC).</summary>
    public DateTimeOffset? DueByUtc { get; init; }
}
