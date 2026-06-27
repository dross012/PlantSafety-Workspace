// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a new Near Miss record.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Create;

/// <summary>
/// Creates a new Near Miss record.
/// Caller must hold <c>SafetyEvents.NearMisses.Create</c>.
/// </summary>
public sealed record CreateNearMissCommand : ICommand<Guid>
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

    /// <summary>Site where the near miss occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the near miss occurred.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Potential severity if the event had escalated.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>Hazard category.</summary>
    public required string HazardCategory { get; init; }

    /// <summary>Worker who is reporting the near miss.</summary>
    public required Guid ReportedByWorkerId { get; init; }
}
