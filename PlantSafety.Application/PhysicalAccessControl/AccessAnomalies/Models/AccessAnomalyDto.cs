// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a single AccessAnomaly record.
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;

/// <summary>A single AccessAnomaly record (system-detected physical-access anomaly).</summary>
public sealed record AccessAnomalyDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AccessAnomalyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Identity of the AccessPoint where the anomaly occurred.</summary>
    public required Guid AccessPointId { get; init; }

    /// <summary>Identity of the Credential involved, if known.</summary>
    public Guid? CredentialId { get; init; }

    /// <summary>Identity of the SafetyZone affected, if known.</summary>
    public Guid? SafetyZoneId { get; init; }

    /// <summary>Classification of the anomaly.</summary>
    public required string AnomalyType { get; init; }

    /// <summary>Severity score (0-100).</summary>
    public required decimal SeverityScore { get; init; }

    /// <summary>Current triage status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the anomaly was detected.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
