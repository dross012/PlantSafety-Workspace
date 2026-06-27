// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to update an existing Safety Observation.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Update;

/// <summary>
/// Updates mutable fields of an existing Safety Observation.
/// Caller must hold <c>SafetyEvents.Observations.Update</c>.
/// </summary>
public sealed record UpdateObservationCommand : ICommand<Guid>, IConcurrencyAwareCommand
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

    /// <inheritdoc/>
    public string? ETag { get; init; }

    /// <summary>Observation to update.</summary>
    public required Guid ObservationId { get; init; }

    /// <summary>Updated title. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated observation type. Null means no change.</summary>
    public string? ObservationType { get; init; }
}
