// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to soft-delete an Incident record.
//   Hard deletes are prohibited; this marks the record as archived/deleted.
//   Optimistic concurrency enforced via ETag.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;

/// <summary>
/// Soft-deletes an Incident by transitioning its status to <c>Archived</c>.
/// Hard deletes are forbidden per the data retention policy.
/// The caller must hold the <c>SafetyEvents.Incidents.Delete</c> permission.
/// </summary>
public sealed record DeleteIncidentCommand : ICommand, IConcurrencyAwareCommand
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

    /// <summary>Incident to soft-delete.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Reason for deletion, required for audit trail.</summary>
    public required string DeletionReason { get; init; }
}
