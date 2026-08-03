// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a single AuditReportSection.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditReport.Models;

/// <summary>Read-model DTO for a single compliance-formatted AuditReport section.</summary>
public sealed record AuditReportSectionDto
{
    /// <summary>Canonical section type code.</summary>
    public required string SectionType { get; init; }

    /// <summary>Human-readable heading.</summary>
    public required string Heading { get; init; }

    /// <summary>Structured content payload (JSON).</summary>
    public required string ContentJson { get; init; }
}
