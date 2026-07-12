// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single corrective/preventive action.
// SchemaVersion: 1

namespace PlantSafety.Application.ContinuousImprovement.CAPA.Models;

/// <summary>A single corrective or preventive action tracked by the CAPA module.</summary>
public sealed record CAPAItemDto
{
    /// <summary>CAPA record identifier.</summary>
    public required Guid CapaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Action classification (Corrective or Preventive).</summary>
    public required string ActionType { get; init; }

    /// <summary>Short action title.</summary>
    public required string Title { get; init; }

    /// <summary>Current action status (e.g., Open/InProgress/PendingVerification/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC due date for the action.</summary>
    public required DateTime DueDateUtc { get; init; }

    /// <summary>Identity of the user the action is assigned to.</summary>
    public required Guid AssignedTo { get; init; }
}
