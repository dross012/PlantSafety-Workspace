// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single internal safety audit record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Models;

/// <summary>A single internal safety audit record, backed by the Auditing aggregate.</summary>
public sealed record InternalSafetyAuditItemDto
{
    /// <summary>Identifier of the Auditing aggregate this record represents.</summary>
    public required Guid AuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Scope of the internal audit (e.g., department, process area).</summary>
    public required string Scope { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the audit was conducted.</summary>
    public required DateTime ConductedAtUtc { get; init; }

    /// <summary>Identity of the user who conducted the audit.</summary>
    public required Guid ConductedBy { get; init; }
}
