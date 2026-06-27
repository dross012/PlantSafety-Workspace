// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a Corrective or Preventive Action (CAPA).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;

/// <summary>
/// Creates a new Corrective or Preventive Action.
/// Caller must hold <c>ContinuousImprovement.CAPA.Create</c>.
/// </summary>
public sealed record CreateCorrectiveActionCommand : ICommand<Guid>
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

    /// <summary>Site where the action is to be implemented.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Action type: Corrective or Preventive.</summary>
    public required string ActionType { get; init; }

    /// <summary>Source event identifier (Incident, NearMiss, AuditFinding, etc.).</summary>
    public required Guid SourceEventId { get; init; }

    /// <summary>Source event type.</summary>
    public required string SourceEventType { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Worker assigned to implement the action.</summary>
    public required Guid AssignedToWorkerId { get; init; }

    /// <summary>Due date (UTC).</summary>
    public required DateTimeOffset DueAtUtc { get; init; }
}
