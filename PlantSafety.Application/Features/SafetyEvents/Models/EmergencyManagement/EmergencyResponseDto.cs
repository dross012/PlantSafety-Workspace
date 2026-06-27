// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for an Emergency Response record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.EmergencyManagement;

/// <summary>
/// Read-side projection of an EmergencyResponse aggregate.
/// Records the activation and execution of emergency response plans.
/// </summary>
public sealed record EmergencyResponseDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid EmergencyResponseId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the emergency occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Linked CrisisEvent identifier, if this response is tied to a declared crisis.</summary>
    public Guid? LinkedCrisisEventId { get; init; }

    /// <summary>Emergency response plan that was activated.</summary>
    public required string ResponsePlanName { get; init; }

    /// <summary>UTC timestamp when the response was activated.</summary>
    public required DateTimeOffset ActivatedAtUtc { get; init; }

    /// <summary>Current status (e.g., Active, Standdown, Completed).</summary>
    public required string Status { get; init; }

    /// <summary>Response coordinator.</summary>
    public required Guid CoordinatorId { get; init; }

    /// <summary>UTC timestamp when stand-down was declared, if applicable.</summary>
    public DateTimeOffset? StandDownAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
