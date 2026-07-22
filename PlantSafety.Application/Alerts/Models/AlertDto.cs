// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Alert.
// SchemaVersion: 1

namespace PlantSafety.Application.Alerts.Models;

/// <summary>Immutable read-model representation of a single Alert.</summary>
public sealed record AlertDto
{
    /// <summary>Alert identifier.</summary>
    public required Guid AlertId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (e.g. Triggered/Acknowledged/Dispatched/Escalated/Resolved/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Catalog category code (e.g. "LifeSafetyEmergency").</summary>
    public required string CategoryCode { get; init; }

    /// <summary>Catalog category display name.</summary>
    public required string CategoryDisplayName { get; init; }

    /// <summary>Severity code (Critical/High/Medium/Low).</summary>
    public required string SeverityCode { get; init; }

    /// <summary>Origin that triggered the alert (Sensor/ComputerVision/Wearable/PanicButton/Human/System).</summary>
    public required string SourceType { get; init; }

    /// <summary>Short human-readable alert title.</summary>
    public required string Title { get; init; }

    /// <summary>Zone/area name where the alert occurred.</summary>
    public required string LocationZoneName { get; init; }

    /// <summary>Optional GPS latitude.</summary>
    public double? Latitude { get; init; }

    /// <summary>Optional GPS longitude.</summary>
    public double? Longitude { get; init; }

    /// <summary>UTC instant the alert was triggered.</summary>
    public required DateTime TriggeredAtUtc { get; init; }

    /// <summary>UTC instant the alert was acknowledged, if acknowledged.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }

    /// <summary>UTC instant the alert was resolved, if resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

    /// <summary>UTC instant the alert was closed, if closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Close-out verification notes, if closed.</summary>
    public string? CloseOutVerification { get; init; }
}
