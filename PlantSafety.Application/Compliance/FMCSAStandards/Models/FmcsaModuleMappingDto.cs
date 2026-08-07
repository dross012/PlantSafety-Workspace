// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.FMCSARequirementModuleMapping
//   row -- the per-module mapping whose Severity takes precedence over FMCSARequirement's own
//   nullable Severity for gap-ranking purposes (see usp_FmcsaAuditGaps_GetForSite.sql's header).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.FMCSAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.FMCSARequirementModuleMapping row.</summary>
public sealed record FmcsaModuleMappingDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid MappingId { get; init; }

    /// <summary>The Compliance.FMCSARequirement this mapping applies to.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>PlantSafety module this requirement is mapped to.</summary>
    public required string ModuleName { get; init; }

    /// <summary>Deterministic rule code that produced this mapping.</summary>
    public required string MappingRuleCode { get; init; }

    /// <summary>Human-readable rationale for the mapping.</summary>
    public required string Rationale { get; init; }

    /// <summary>Severity for this module mapping -- authoritative for gap-ranking, distinct from FMCSARequirement.Severity.</summary>
    public required string Severity { get; init; }
}
