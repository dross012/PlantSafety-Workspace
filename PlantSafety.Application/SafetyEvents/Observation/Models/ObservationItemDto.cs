// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single safety observation.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.Observation.Models;

/// <summary>A single safety observation recorded by an observer.</summary>
public sealed record ObservationItemDto
{
    /// <summary>Observation identifier.</summary>
    public required Guid ObservationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Observation classification (e.g., SafeAct/AtRiskAct/SafeCondition/AtRiskCondition).</summary>
    public required string ObservationType { get; init; }

    /// <summary>Observation category (e.g., PPE/Housekeeping/Ergonomics/Procedure).</summary>
    public required string Category { get; init; }

    /// <summary>Current observation status (e.g., Open/ActionAssigned/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the observation was made.</summary>
    public required DateTime ObservedAtUtc { get; init; }

    /// <summary>Identity of the user who made the observation.</summary>
    public required Guid ObservedBy { get; init; }
}
