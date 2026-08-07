// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: A single "gap": an NFPA hazard mapped to a module that has never been linked
//   (via Auditing.AuditFindingNfpaCoverage) from any AuditFinding at a given site.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

/// <summary>A single NFPA hazard mapping with no covering AuditFinding at the queried site.</summary>
public sealed record NfpaGapDto
{
    /// <summary>Identity of the uncovered mapping.</summary>
    public required Guid MappingId { get; init; }

    /// <summary>PlantSafety module this mapping applies to.</summary>
    public required string ModuleName { get; init; }

    /// <summary>
    /// Deterministic rule code that produced this mapping, when the backing query returns it.
    /// Null on rows from usp_NfpaDashboard_GetTopUncovered, which does not select this column
    /// (that procedure spans every module and omits per-mapping rule provenance by design).
    /// </summary>
    public string? MappingRuleCode { get; init; }

    /// <summary>Advisory/Low/Medium/High/Critical severity assigned to this mapping.</summary>
    public required string Severity { get; init; }

    /// <summary>Rationale for why this hazard was mapped to this module.</summary>
    public required string Rationale { get; init; }

    /// <summary>Identity of the underlying hazard profile.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>NFPA code number of the hazard (e.g. "70").</summary>
    public required string CodeNumber { get; init; }

    /// <summary>Section citation of the hazard.</summary>
    public required string SectionNumber { get; init; }

    /// <summary>Hazard name, for display context.</summary>
    public required string HazardName { get; init; }
}
