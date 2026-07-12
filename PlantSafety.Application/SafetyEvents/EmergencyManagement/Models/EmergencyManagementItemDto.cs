// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single emergency management event.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

/// <summary>A single emergency event tracked by the EmergencyManagement module.</summary>
public sealed record EmergencyManagementItemDto
{
    /// <summary>Emergency event identifier.</summary>
    public required Guid EmergencyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of emergency (e.g., Fire/ChemicalRelease/MedicalEmergency/SevereWeather).</summary>
    public required string EmergencyType { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current emergency status (e.g., Reported/Responding/Contained/Resolved).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the emergency was reported.</summary>
    public required DateTime ReportedAtUtc { get; init; }

    /// <summary>Identity of the user who reported the emergency.</summary>
    public required Guid ReportedBy { get; init; }
}
