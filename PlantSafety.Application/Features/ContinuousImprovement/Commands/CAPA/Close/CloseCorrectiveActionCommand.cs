// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Command to verify and formally close an existing CorrectiveAction (terminal
// state).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Close;

/// <summary>
/// Verifies and formally closes an existing CorrectiveAction (terminal state).
/// Caller must hold <c>ContinuousImprovement.CAPA.Update</c>.
/// </summary>
public sealed record CloseCorrectiveActionCommand : ICommand<Guid>
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

    /// <summary>CorrectiveAction to close.</summary>
    public required Guid CaId { get; init; }
}
