// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single AutonomousVehicleIncident record.
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Models;

/// <summary>A single AutonomousVehicleIncident record.</summary>
public sealed record AutonomousVehicleIncidentItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AutonomousVehicleIncidentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the autonomous vehicle asset involved in the incident.</summary>
    public required Guid AutonomousVehicleAssetId { get; init; }

    /// <summary>Incident classification.</summary>
    public required string IncidentType { get; init; }

    /// <summary>Severity classification.</summary>
    public required string SeverityLevel { get; init; }

    /// <summary>Overall lifecycle status classification.</summary>
    public required string Status { get; init; }

    /// <summary>Indicates whether a suspension review has been triggered for this incident.</summary>
    public required bool SuspensionTriggered { get; init; }

    /// <summary>UTC timestamp the incident occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
