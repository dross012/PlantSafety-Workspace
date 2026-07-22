// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Alert response action.
// SchemaVersion: 1

namespace PlantSafety.Application.Alerts.Models;

/// <summary>Immutable read-model representation of a single Alert response action.</summary>
public sealed record AlertResponseActionDto
{
    /// <summary>Response action identifier.</summary>
    public required Guid ResponseActionId { get; init; }

    /// <summary>Identifier of the parent Alert.</summary>
    public required Guid AlertId { get; init; }

    /// <summary>Short description of the action taken.</summary>
    public required string ActionDescription { get; init; }

    /// <summary>UTC instant the action was taken.</summary>
    public required DateTime TakenAtUtc { get; init; }

    /// <summary>Identity descriptor of who/what took the action.</summary>
    public required string TakenBy { get; init; }

    /// <summary>Optional free-text notes about the action.</summary>
    public string? Notes { get; init; }
}
