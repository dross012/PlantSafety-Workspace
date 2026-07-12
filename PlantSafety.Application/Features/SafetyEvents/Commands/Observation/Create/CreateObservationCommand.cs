// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a Safety Observation record.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Create;

/// <summary>
/// Creates a new Safety Observation.
/// Caller must hold <c>SafetyEvents.Observations.Create</c>.
/// </summary>
public sealed record CreateObservationCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site where the observation was made.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Observation type: Positive, AtRisk, Unsafe.</summary>
    public required string ObservationType { get; init; }

    /// <summary>UTC timestamp when the observation was made.</summary>
    public required DateTimeOffset ObservedAtUtc { get; init; }

    /// <summary>Worker who made the observation.</summary>
    public required Guid ObservedByWorkerId { get; init; }
}
