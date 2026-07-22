// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to instantly notify the assigned supervisor of an automated incident
// detection ("Instant notifications to supervisors").
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Notifies the assigned supervisor of an AutomatedIncidentDetection.</summary>
public sealed record NotifySupervisorOfDetectionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid AutomatedIncidentDetectionId,
    Guid SupervisorUserId,
    Guid PerformedByUserId) : ICommand<Result<AutomatedIncidentDetectionActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AutomatedIncidentDetectionActionResult>>.CausationId => CausationId;
}
