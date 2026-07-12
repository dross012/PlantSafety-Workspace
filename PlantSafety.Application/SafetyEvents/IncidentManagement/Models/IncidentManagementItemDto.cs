// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single safety incident.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.IncidentManagement.Models;

/// <summary>A single safety incident tracked by the IncidentManagement module.</summary>
public sealed record IncidentManagementItemDto
{
    /// <summary>Incident identifier.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of incident (e.g., Injury/PropertyDamage/EnvironmentalRelease).</summary>
    public required string IncidentType { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current incident status (e.g., Reported/UnderInvestigation/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the incident occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>Identity of the user who reported the incident.</summary>
    public required Guid ReportedBy { get; init; }
}
