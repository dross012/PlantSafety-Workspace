// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single derived OSHA requirement sentence,
//   projected from Compliance.OSHARequirement. AppliesToModule is populated by a separate
//   mapping pass (not by tools/OshaStandardsImport, which always leaves it NULL on import) --
//   see OSHARequirement.sql's header. Do not assume it is populated.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.OSHARequirement row.</summary>
public sealed record OSHARequirementItemDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid OSHARequirementId { get; init; }

    /// <summary>The Compliance.OSHAStandard this requirement was derived from.</summary>
    public required Guid OSHAStandardId { get; init; }

    /// <summary>Keyword-derived category (PPE, Training, Inspection, Recordkeeping, Reporting, Procedure, Permit, General). See tools/OshaStandardsImport's TypeKeywords table for the exact derivation.</summary>
    public string? RequirementType { get; init; }

    /// <summary>PlantSafety module this requirement has been mapped to. NULL until a mapping pass populates it -- never fabricated at import time.</summary>
    public string? AppliesToModule { get; init; }

    /// <summary>Severity, when explicitly assigned by a mapping pass. NULL on import.</summary>
    public string? Severity { get; init; }

    /// <summary>The raw source sentence this requirement was derived from (never synthesized).</summary>
    public required string Notes { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
