// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to activate an Emergency Response.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.EmergencyManagement.Create;

/// <summary>
/// Activates an Emergency Response plan.
/// Caller must hold <c>SafetyEvents.EmergencyManagement.Activate</c>.
/// </summary>
public sealed record CreateEmergencyResponseCommand : ICommand<Guid>
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

    /// <summary>Site where the emergency response is active.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Optional linked crisis event.</summary>
    public Guid? LinkedCrisisEventId { get; init; }

    /// <summary>Name of the emergency response plan activated.</summary>
    public required string ResponsePlanName { get; init; }

    /// <summary>Description of the emergency and response. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp of activation.</summary>
    public required DateTimeOffset ActivatedAtUtc { get; init; }

    /// <summary>Response coordinator.</summary>
    public required Guid CoordinatorId { get; init; }
}
