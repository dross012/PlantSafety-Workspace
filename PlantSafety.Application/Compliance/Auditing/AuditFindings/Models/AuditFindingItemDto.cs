// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single audit finding.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Auditing.AuditFindings.Models;

/// <summary>A single finding recorded against an Auditing aggregate.</summary>
public sealed record AuditFindingItemDto
{
    /// <summary>Finding identifier.</summary>
    public required Guid FindingId { get; init; }

    /// <summary>Identifier of the parent Auditing aggregate this finding was recorded against.</summary>
    public required Guid AuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short finding title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current finding status (e.g., Open/InRemediation/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the finding was identified.</summary>
    public required DateTime IdentifiedAtUtc { get; init; }

    /// <summary>Identity of the user who identified the finding.</summary>
    public required Guid IdentifiedBy { get; init; }
}
