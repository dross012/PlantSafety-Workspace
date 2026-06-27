// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to update an existing Near Miss record.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Update;

/// <summary>
/// Updates mutable fields of an existing Near Miss.
/// Caller must hold <c>SafetyEvents.NearMisses.Update</c>.
/// Requires a valid <see cref="ETag"/> for optimistic concurrency.
/// </summary>
public sealed record UpdateNearMissCommand : ICommand<Guid>, IConcurrencyAwareCommand
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

    /// <summary>Near miss to update.</summary>
    public required Guid NearMissId { get; init; }

    /// <summary>Updated title. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated potential severity. Null means no change.</summary>
    public string? PotentialSeverity { get; init; }

    /// <summary>Updated hazard category. Null means no change.</summary>
    public string? HazardCategory { get; init; }
}
