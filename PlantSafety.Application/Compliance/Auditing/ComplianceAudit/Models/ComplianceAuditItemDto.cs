// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single compliance audit record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Models;

/// <summary>A single compliance audit record, backed by the Auditing aggregate.</summary>
public sealed record ComplianceAuditItemDto
{
    /// <summary>Identifier of the Auditing aggregate this record represents.</summary>
    public required Guid AuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Audit type/standard (e.g., ISO45001, OSHA, Internal).</summary>
    public required string AuditType { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the audit was conducted.</summary>
    public required DateTime ConductedAtUtc { get; init; }

    /// <summary>Identity of the user who conducted the audit.</summary>
    public required Guid ConductedBy { get; init; }
}
