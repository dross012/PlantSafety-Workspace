// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to update an existing Incident record.
//   Optimistic concurrency enforced via ETag.
//   Returns the updated IncidentId (same as input, included for consistent Result envelope).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;

/// <summary>
/// Updates mutable fields of an existing Incident.
/// The caller must hold the <c>SafetyEvents.Incidents.Update</c> permission.
/// Requires a valid <see cref="ETag"/> — stale ETags result in a <c>ConcurrencyConflictException</c>.
/// </summary>
public sealed record UpdateIncidentCommand : ICommand<Guid>, IConcurrencyAwareCommand
{
    // --- Canonical envelope ---

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

    // --- Concurrency ---

    /// <inheritdoc/>
    public string? ETag { get; init; }

    // --- Domain payload ---

    /// <summary>Incident to update.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Updated title. Max 200 characters. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Max 4000 characters. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated severity. Null means no change.</summary>
    public string? Severity { get; init; }

    /// <summary>Updated location. Null means no change.</summary>
    public string? Location { get; init; }

    /// <summary>Updated investigator assignment. Null means no change; empty Guid means unassign.</summary>
    public Guid? AssignedInvestigatorId { get; init; }
}
