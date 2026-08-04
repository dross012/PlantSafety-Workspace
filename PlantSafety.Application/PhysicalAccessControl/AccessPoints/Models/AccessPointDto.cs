// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a single AccessPoint record.
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.AccessPoints.Models;

/// <summary>A single AccessPoint record (badge reader, turnstile, mantrap door, or vehicle gate).</summary>
public sealed record AccessPointDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AccessPointId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Access-point code/label.</summary>
    public required string AccessPointCode { get; init; }

    /// <summary>Narrative description / physical location.</summary>
    public string? Description { get; init; }

    /// <summary>Physical device classification.</summary>
    public required string DeviceType { get; init; }

    /// <summary>Identity of the SafetyZone this access point guards, if any.</summary>
    public Guid? SafetyZoneId { get; init; }

    /// <summary>Whether this access point is on the site perimeter.</summary>
    public bool IsPerimeter { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
