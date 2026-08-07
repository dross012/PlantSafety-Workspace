// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: A single "gap": an OSHA requirement mapped to a module that has never been
//   linked (via Auditing.AuditFindingOshaCoverage) from any AuditFinding at a given site.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;

/// <summary>A single OSHA requirement with no covering AuditFinding at the queried site.</summary>
public sealed record OshaGapDto
{
    /// <summary>Identity of the uncovered requirement.</summary>
    public required Guid OSHARequirementId { get; init; }

    /// <summary>Category derived at import time (PPE/Training/Inspection/etc.).</summary>
    public string? RequirementType { get; init; }

    /// <summary>Severity assigned by the module-mapping pass.</summary>
    public required string Severity { get; init; }

    /// <summary>The raw source sentence this requirement was derived from.</summary>
    public required string Notes { get; init; }

    /// <summary>CFR part number of the standard this requirement belongs to.</summary>
    public required int PartNumber { get; init; }

    /// <summary>Section citation of the standard this requirement belongs to.</summary>
    public required string Section { get; init; }

    /// <summary>Section heading, for display context.</summary>
    public string? SectionTitle { get; init; }
}
