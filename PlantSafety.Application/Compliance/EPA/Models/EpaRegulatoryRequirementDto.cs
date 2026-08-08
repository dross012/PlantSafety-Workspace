// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.EpaRegulatoryRequirement row.
//   Global reference data, not tenant-scoped. ImpactedModules is the raw comma-delimited
//   free-text column value (matching the OSHARequirement.AppliesToModule convention) --
//   callers that need module-scoped filtering should prefer
//   GetEpaRequirementsByModuleQuery rather than parsing this client-side.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.EPA.Models;

/// <summary>Flat read-model DTO for a single Compliance.EpaRegulatoryRequirement row.</summary>
public sealed record EpaRegulatoryRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>The EPA program this requirement belongs to (e.g. "RCRA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>Short title of the requirement.</summary>
    public required string Title { get; init; }

    /// <summary>Regulatory citation (e.g. "40 CFR 262.17").</summary>
    public required string Citation { get; init; }

    /// <summary>Full requirement text.</summary>
    public required string Description { get; init; }

    /// <summary>Narrative applicability conditions, when the requirement doesn't apply universally within the program.</summary>
    public string? ApplicabilityConditions { get; init; }

    /// <summary>Comma-delimited free-text list of PlantSafety module names this requirement affects.</summary>
    public required string ImpactedModules { get; init; }

    /// <summary>Severity code (Advisory/Low/Medium/High/Critical), matching PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity.</summary>
    public required string Severity { get; init; }

    /// <summary>Optional link to the authoritative source (e.g. eCFR).</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
