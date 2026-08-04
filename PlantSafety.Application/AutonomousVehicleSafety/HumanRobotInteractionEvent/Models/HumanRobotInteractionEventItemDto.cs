// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single HumanRobotInteractionEvent record.
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Models;

/// <summary>A single HumanRobotInteractionEvent record (proximity alert, near miss, contact, or e-stop trigger).</summary>
public sealed record HumanRobotInteractionEventItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HumanRobotInteractionEventId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the autonomous vehicle asset involved in the interaction.</summary>
    public required Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Interaction classification.</summary>
    public required string InteractionType { get; init; }

    /// <summary>Severity classification.</summary>
    public required string SeverityLevel { get; init; }

    /// <summary>Measured separation distance in meters, if available.</summary>
    public decimal? DistanceMeters { get; init; }

    /// <summary>Overall lifecycle status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the interaction was detected.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
