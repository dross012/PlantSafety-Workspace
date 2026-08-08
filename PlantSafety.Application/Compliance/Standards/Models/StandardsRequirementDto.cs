// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.StandardsRequirement row.
//   Global reference data, not tenant-scoped. ImpactedModules is the raw comma-delimited
//   free-text column value (matching the EpaRegulatoryRequirementDto/OSHARequirement
//   convention) -- callers that need module-scoped filtering should prefer
//   GetStandardsByModuleQuery rather than parsing this client-side.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Standards.Models;

/// <summary>Flat read-model DTO for a single Compliance.StandardsRequirement row.</summary>
public sealed record StandardsRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>The standards family this requirement belongs to (e.g. "ISO").</summary>
    public required string FamilyCode { get; init; }

    /// <summary>The standards series this requirement belongs to (e.g. "9001").</summary>
    public required string SeriesCode { get; init; }

    /// <summary>Short title of the requirement.</summary>
    public required string Title { get; init; }

    /// <summary>Full requirement text.</summary>
    public required string Description { get; init; }

    /// <summary>Narrative applicability conditions, when the requirement doesn't apply universally within the series.</summary>
    public string? ApplicabilityConditions { get; init; }

    /// <summary>Comma-delimited free-text list of PlantSafety module names this requirement affects.</summary>
    public required string ImpactedModules { get; init; }

    /// <summary>Severity code (Advisory/Low/Medium/High/Critical), matching PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity.</summary>
    public required string Severity { get; init; }

    /// <summary>Optional link to the authoritative source.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
