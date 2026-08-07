// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: A single "gap": an FMCSA requirement mapped to a module that has never been
//   linked (via Auditing.AuditFindingFmcsaCoverage) from any AuditFinding at a given site.
//   ModuleName is populated when the source query joins FMCSARequirementModuleMapping
//   (usp_FmcsaAuditGaps_GetForSite) but is null when the source query has no module dimension
//   (usp_FmcsaDashboard_GetTopUncovered).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;

/// <summary>A single FMCSA requirement with no covering AuditFinding at the queried site.</summary>
public sealed record FmcsaGapDto
{
    /// <summary>Identity of the uncovered requirement.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>Requirement citation/number, when assigned.</summary>
    public string? RequirementNumber { get; init; }

    /// <summary>Severity driving this gap's ranking (module-mapping severity when a module dimension is present, otherwise the requirement's own severity).</summary>
    public required string Severity { get; init; }

    /// <summary>The requirement's source text.</summary>
    public required string RequirementText { get; init; }

    /// <summary>PlantSafety module this gap applies to, when the source query has a module dimension.</summary>
    public string? ModuleName { get; init; }

    /// <summary>CFR part number of the standard this requirement belongs to.</summary>
    public required string PartNumber { get; init; }

    /// <summary>Section citation of the standard this requirement belongs to.</summary>
    public required string SectionNumber { get; init; }

    /// <summary>Section heading, for display context.</summary>
    public string? SectionTitle { get; init; }
}
