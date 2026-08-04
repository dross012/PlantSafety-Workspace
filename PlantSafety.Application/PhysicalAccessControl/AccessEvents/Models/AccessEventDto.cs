// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a single AccessEvent record.
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;

/// <summary>A single AccessEvent record ingested from an external badge system.</summary>
public sealed record AccessEventDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AccessEventId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Identity of the AccessPoint that produced this event.</summary>
    public required Guid AccessPointId { get; init; }

    /// <summary>Identity of the Credential presented, if any.</summary>
    public Guid? CredentialId { get; init; }

    /// <summary>Identity of the SafetyZone affected, if known.</summary>
    public Guid? SafetyZoneId { get; init; }

    /// <summary>Raw event outcome as reported by the badge system.</summary>
    public required string EventType { get; init; }

    /// <summary>Direction of travel.</summary>
    public required string Direction { get; init; }

    /// <summary>Processing status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the underlying physical event occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }
}
