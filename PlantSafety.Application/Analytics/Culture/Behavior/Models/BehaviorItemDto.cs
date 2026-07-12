// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single safety behavior observation.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Culture.Behavior.Models;

/// <summary>A single behavior observation recorded as part of the safety culture program.</summary>
public sealed record BehaviorItemDto
{
    /// <summary>Observation identifier.</summary>
    public required Guid ObservationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the employee observed.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Behavior category (e.g., PPE Compliance, Housekeeping, Ergonomics).</summary>
    public required string BehaviorCategory { get; init; }

    /// <summary>Observation classification (e.g., Safe, AtRisk).</summary>
    public required string ObservationType { get; init; }

    /// <summary>Current status of the observation (e.g., Open, Reviewed, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the observation was recorded.</summary>
    public required DateTime ObservedAtUtc { get; init; }

    /// <summary>Identity of the observer who recorded the entry.</summary>
    public required Guid ObservedBy { get; init; }
}
