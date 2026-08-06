// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Command to mark an existing CorrectiveAction Overdue when its due date has
// passed without completion.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.MarkOverdue;

/// <summary>
/// Marks an existing CorrectiveAction Overdue when its due date has passed without completion.
/// No-op (idempotent) if the action is already Closed/AwaitingVerification.
/// Caller must hold <c>ContinuousImprovement.CAPA.Update</c>.
/// </summary>
public sealed record MarkCorrectiveActionOverdueCommand : ICommand<Guid>
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

    /// <summary>CorrectiveAction to mark overdue.</summary>
    public required Guid CaId { get; init; }
}
