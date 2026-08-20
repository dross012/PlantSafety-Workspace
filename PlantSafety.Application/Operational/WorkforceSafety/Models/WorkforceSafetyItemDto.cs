// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Read-model item DTO for a single WorkforceSafety record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.WorkforceSafety.Models;

/// <summary>A single WorkforceSafety read-model record.</summary>
public sealed record WorkforceSafetyItemDto
{
    /// <summary>WorkforceSafety record identifier.</summary>
    public required Guid WorkforceSafetyRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the workforce safety metric.</summary>
    public required Guid MetricId { get; init; }

    /// <summary>Display name of the workforce safety metric (e.g. Near-Miss Rate, PPE Compliance Rate).</summary>
    public required string MetricName { get; init; }

    /// <summary>Measured metric value.</summary>
    public required decimal MetricValue { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the user or system that last recorded this metric.</summary>
    public required Guid RecordedBy { get; init; }
}