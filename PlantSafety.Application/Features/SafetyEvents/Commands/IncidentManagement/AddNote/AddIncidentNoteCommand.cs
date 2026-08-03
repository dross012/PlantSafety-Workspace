// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to add a technician/operator note to an existing Incident.
//   Backs the v1/safety-events/incidents/{id}/notes route (UI Gaps Scoping, IncidentManagement Part 1).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.AddNote;

/// <summary>
/// Adds a technician/operator note to an existing Incident's timeline.
/// The caller must hold the <c>SafetyEvents.Incidents.Update</c> permission.
/// </summary>
public sealed record AddIncidentNoteCommand : ICommand<Guid>
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

    /// <summary>Incident to add a note to.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Note text.</summary>
    public required string Text { get; init; }
}
