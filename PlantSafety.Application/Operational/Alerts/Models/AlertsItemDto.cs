// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Alerts record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Alerts.Models;

/// <summary>A single Alerts read-model record.</summary>
public sealed record AlertsItemDto
{
    /// <summary>Alerts record identifier.</summary>
    public required Guid AlertId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Alert message text.</summary>
    public required string Message { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime TriggeredAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid AcknowledgedBy { get; init; }
}
