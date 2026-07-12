// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single CAPA trigger.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Auditing.CAPATriggers.Models;

/// <summary>A single corrective/preventive action trigger raised from an audit finding.</summary>
public sealed record CAPATriggerItemDto
{
    /// <summary>Trigger identifier.</summary>
    public required Guid TriggerId { get; init; }

    /// <summary>Identifier of the parent Auditing aggregate this trigger stems from.</summary>
    public required Guid AuditingId { get; init; }

    /// <summary>Identifier of the source finding that raised this trigger, if any.</summary>
    public Guid? SourceFindingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reason the CAPA was triggered.</summary>
    public required string Reason { get; init; }

    /// <summary>Current trigger status (e.g., Open/InProgress/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the trigger was raised.</summary>
    public required DateTime TriggeredAtUtc { get; init; }

    /// <summary>Identity of the user who raised the trigger.</summary>
    public required Guid TriggeredBy { get; init; }
}
