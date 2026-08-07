// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Composite read-model DTO for a single hazard-mapped-to-module row, projected
//   from the join usp_NfpaHazards_GetByModule performs between Compliance.NFPA_HazardProfile and
//   Compliance.NFPA_HazardModuleMapping. Design choice: rather than returning a nested
//   NfpaHazardProfileDto with an attached NfpaModuleMappingDto (which would require inventing an
//   arbitrary containment shape the stored procedure's single flat result set doesn't reflect),
//   this DTO flattens both into one record, one instance per (hazard, mapping) row -- the same
//   cardinality the SQL join already returns. This mirrors how OSHARequirementItemDto carries its
//   own AppliesToModule/Severity fields directly rather than nesting a separate mapping object.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Models;

/// <summary>Flat, joined read-model DTO for one NFPA hazard mapped to one PlantSafety module.</summary>
public sealed record NfpaHazardModuleDto
{
    /// <summary>Surrogate identity of the hazard.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>NFPA code number (e.g. "70").</summary>
    public required string CodeNumber { get; init; }

    /// <summary>Section citation within the code.</summary>
    public required string SectionNumber { get; init; }

    /// <summary>Short hazard name/title.</summary>
    public required string HazardName { get; init; }

    /// <summary>Narrative description of the hazard.</summary>
    public required string HazardDescription { get; init; }

    /// <summary>Recommended controls for this hazard, when documented.</summary>
    public string? RecommendedControls { get; init; }

    /// <summary>Recommended PPE for this hazard, when documented.</summary>
    public string? PPERecommendations { get; init; }

    /// <summary>Electrical safety guidance specific to this hazard, when documented.</summary>
    public string? ElectricalSafetyGuidance { get; init; }

    /// <summary>Hot work guidance specific to this hazard, when documented.</summary>
    public string? HotWorkGuidance { get; init; }

    /// <summary>Flammable/combustible materials guidance specific to this hazard, when documented.</summary>
    public string? FlammableMaterialsGuidance { get; init; }

    /// <summary>Source URL the hazard profile was imported from.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When the hazard profile row was first imported.</summary>
    public required DateTime CreatedOn { get; init; }

    /// <summary>SHA-256 hash of the source section text, when hashed.</summary>
    public string? SectionTextHash { get; init; }

    /// <summary>Surrogate identity of the module mapping row.</summary>
    public required Guid MappingId { get; init; }

    /// <summary>Deterministic rule code that produced this mapping.</summary>
    public required string MappingRuleCode { get; init; }

    /// <summary>Rationale for why this hazard was mapped to this module.</summary>
    public required string Rationale { get; init; }

    /// <summary>Advisory/Low/Medium/High/Critical -- the relative severity of this mapping.</summary>
    public required string Severity { get; init; }
}
