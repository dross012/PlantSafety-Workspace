// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to declare a new Crisis Management event.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.CrisisManagement.Create;

/// <summary>
/// Declares a new Crisis Management event.
/// Caller must hold <c>SafetyEvents.CrisisManagement.Declare</c> — a restricted, role-gated permission.
/// Publishes <c>SafetyEvents.CrisisDeclared.v1</c> via the outbox on success.
/// </summary>
public sealed record CreateCrisisEventCommand : ICommand<Guid>
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

    /// <summary>Site affected.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Crisis category (e.g., Fire, ChemicalSpill, StructuralFailure, MassInjury).</summary>
    public required string CrisisCategory { get; init; }

    /// <summary>UTC timestamp when the crisis was declared.</summary>
    public required DateTimeOffset DeclaredAtUtc { get; init; }

    /// <summary>Worker serving as Incident Commander.</summary>
    public required Guid IncidentCommanderId { get; init; }
}
