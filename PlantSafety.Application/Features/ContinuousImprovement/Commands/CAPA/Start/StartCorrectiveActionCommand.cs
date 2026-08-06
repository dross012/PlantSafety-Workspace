// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Command to transition an existing CorrectiveAction from Open to InProgress.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Start;

/// <summary>
/// Transitions an existing CorrectiveAction from Open to InProgress.
/// Caller must hold <c>ContinuousImprovement.CAPA.Update</c>.
/// </summary>
public sealed record StartCorrectiveActionCommand : ICommand<Guid>
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

    /// <summary>CorrectiveAction to start.</summary>
    public required Guid CaId { get; init; }
}
