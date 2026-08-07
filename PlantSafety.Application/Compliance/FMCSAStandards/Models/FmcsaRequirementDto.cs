// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single derived FMCSA requirement sentence,
//   projected from Compliance.FMCSARequirement. AppliesToModule/Severity here are the
//   denormalized columns on FMCSARequirement itself -- distinct from, and less authoritative
//   than for gap-ranking purposes, the per-module Severity carried on
//   Compliance.FMCSARequirementModuleMapping (see FmcsaGapDto/FmcsaGapSummaryDto).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.FMCSAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.FMCSARequirement row.</summary>
public sealed record FmcsaRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>The Compliance.FMCSAStandard this requirement was derived from.</summary>
    public required Guid StandardId { get; init; }

    /// <summary>Requirement citation/number, when assigned.</summary>
    public string? RequirementNumber { get; init; }

    /// <summary>The requirement's source text.</summary>
    public required string RequirementText { get; init; }

    /// <summary>Advisory/Low/Medium/High/Critical, when explicitly assigned. NULL until rated.</summary>
    public string? Severity { get; init; }

    /// <summary>PlantSafety module this requirement has been mapped to. NULL until a mapping pass populates it.</summary>
    public string? AppliesToModule { get; init; }

    /// <summary>Rationale for the module mapping/severity rating, when available.</summary>
    public string? Rationale { get; init; }
}
