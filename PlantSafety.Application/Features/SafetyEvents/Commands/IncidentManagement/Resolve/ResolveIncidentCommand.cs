// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to resolve an existing Incident with required resolution notes.
//   Backs the v1/safety-events/incidents/{id}/resolve route (UI Gaps Scoping, IncidentManagement Part 1).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Resolve;

/// <summary>
/// Resolves an existing Incident with required resolution notes.
/// The caller must hold the <c>SafetyEvents.Incidents.Update</c> permission.
/// </summary>
public sealed record ResolveIncidentCommand : ICommand<Guid>
{
    // --- Canonical envelope ---

    /// <summary>Unique identifier of this command instance.</summary>
    public required Guid CommandId { get; init; }

    /// <summary>Correlation identifier for the originating request chain.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Identifier of the event/command that caused this command, if any.</summary>
    public required Guid? CausationId { get; init; }

    /// <summary>UTC instant the command was requested.</summary>
    public required DateTime RequestedAtUtc { get; init; }

    /// <summary>API version token of the originating request.</summary>
    public required string ApiVersionToken { get; init; }

    /// <summary>Tenant issuing the command.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Actor issuing the command.</summary>
    public required Guid ActorId { get; init; }

    // --- Domain payload ---

    /// <summary>Incident to resolve.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Resolution notes.</summary>
    public required string ResolutionNotes { get; init; }
}
