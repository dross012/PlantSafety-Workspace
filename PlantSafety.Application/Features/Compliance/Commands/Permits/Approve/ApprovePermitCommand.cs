// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to approve a Permit to Work, transitioning it to Active status.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;

namespace PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;

/// <summary>
/// Approves a Permit to Work, transitioning it from PendingApproval to Active.
/// Caller must hold <c>Compliance.Permits.Approve</c>.
/// </summary>
public sealed record ApprovePermitCommand : ICommand<Guid>, IConcurrencyAwareCommand
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

    /// <inheritdoc/>
    public string? ETag { get; init; }

    /// <summary>Permit to approve.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Optional approval notes.</summary>
    public string? ApprovalNotes { get; init; }
}
